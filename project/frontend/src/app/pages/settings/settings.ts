import { Component, inject, OnInit, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Sidebar } from "../../components/sidebar/sidebar";
import { GenericInput } from "../../components/input/generic-input/generic-input";
import { GenericButton } from "../../components/buttons/generic-button";
import { UserDTO, UserService } from "../../core/user/user.service";
import { compressImage } from "../../core/utils/image-compressor";
import { CityAutocomplete } from "../../components/input/city-autocomplete/city-autocomplete";
import { AuthService } from "../../core/auth/auth.service";
import { Router } from "@angular/router";

type TabId = 'account';

interface UserForm {
    completeName: FormControl<string>;
    email: FormControl<string>;
    biography: FormControl<string>;
    birthDate: FormControl<string>;
    city: FormControl<string>;
}

@Component({
    selector: 'settings',
    imports: [Sidebar, GenericInput, CityAutocomplete, ReactiveFormsModule, GenericButton],
    templateUrl: './settings.html',
})
export class Settings implements OnInit {
    private readonly userService = inject(UserService);
    private readonly authService = inject(AuthService);
    private readonly router = inject(Router);

    readonly tabs: { id: TabId; label: string; icon: string; color: string }[] = [
        { id: 'account', label: 'Conta', icon: 'icons/user.svg', color: 'text-primary' },
    ];

    readonly form = new FormGroup<UserForm>({
        completeName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
        email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
        biography: new FormControl('', { nonNullable: true }),
        birthDate: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
        city: new FormControl('', { nonNullable: true }),
    });

    constructor() {
        this.form.controls.email.disable();
    }

    activeTab = signal<TabId>('account');
    loading = signal(true);
    saving = signal(false);
    saveError = signal<string | null>(null);

    readonly photoUrl = signal<string | null>(null);
    readonly uploadingPhoto = signal(false);
    readonly photoError = signal<string | null>(null);

    private readonly allowedImageTypes = ['image/jpeg', 'image/png', 'image/webp'];
    private readonly maxOriginalSize = 25 * 1024 * 1024;
    private readonly maxImageSize = 5 * 1024 * 1024;

    ngOnInit(): void {
        this.loadUserDetails();
    }

    setActiveTab(value: TabId): void {
        this.activeTab.set(value);
    }

    loadUserDetails(): void {

        this.userService.getMe().subscribe({
            next: (user) => {
                this.setUserDetails(user);
                this.loading.set(false);
            },
            error: (error) => {
                console.error(error);
                this.loading.set(false);
            },
        });
    }

    private setUserDetails(user: UserDTO): void {
        this.photoUrl.set(user.urlPhoto);

        this.form.patchValue({
            completeName: user.name,
            email: user.email,
            biography: user.description ?? '',
            birthDate: user.birthday?.slice(0, 10) ?? '',
            city: user.city ?? '',
        });

        this.form.markAsPristine();
    }

    async onPhotoSelected(event: Event): Promise<void> {
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0] ?? null;
        input.value = '';

        if (!file) {
            return;
        }

        if (!this.allowedImageTypes.includes(file.type)) {
            this.photoError.set('Formato inválido. Use JPG, PNG ou WEBP.');
            return;
        }

        if (file.size > this.maxOriginalSize) {
            this.photoError.set('A imagem é muito grande (máximo 25 MB).');
            return;
        }

        let finalFile: File;

        try {
            finalFile = await compressImage(file, { maxDimension: 512, aspectRatio: 1, quality: 0.85 });
        } catch {
            this.photoError.set('Não foi possível processar essa imagem. Tente outra.');
            return;
        }

        if (finalFile.size > this.maxImageSize) {
            this.photoError.set('A imagem continua acima de 5 MB. Tente uma menor.');
            return;
        }

        this.photoError.set(null);
        this.uploadingPhoto.set(true);

        this.userService.uploadPhoto(finalFile).subscribe({
            next: (user) => {
                this.photoUrl.set(user.urlPhoto);
                this.uploadingPhoto.set(false);
            },
            error: (error) => {
                console.error(error);
                this.photoError.set(
                    error?.status === 413
                        ? 'A imagem deve ter no máximo 5 MB.'
                        : error?.error?.errors?.[0] ?? 'Não foi possível enviar a foto. Tente novamente.',
                );
                this.uploadingPhoto.set(false);
            },
        });
    }

    save(): void {
        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        const values = this.form.getRawValue();

        this.saving.set(true);
        this.saveError.set(null);

        this.userService
            .updateMe({
                name: values.completeName.trim(),
                birthday: values.birthDate,
                description: values.biography.trim() || null,
                city: values.city.trim() || null,
            })
            .subscribe({
                next: (user) => {
                    this.setUserDetails(user);
                    this.saving.set(false);
                },
                error: (error) => {
                    console.error(error);
                    this.saveError.set(error?.error?.errors?.[0] ?? 'Não foi possível salvar. Tente novamente.');
                    this.saving.set(false);
                },
            });
    }

    logout(): void {
        this.authService.logout();
        this.router.navigate(['/login']);
    }
}