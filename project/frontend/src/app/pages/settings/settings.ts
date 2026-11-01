import { Component, inject, OnInit, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Sidebar } from "../../components/sidebar/sidebar";
import { GenericInput } from "../../components/input/generic-input/generic-input";
import { GenericButton } from "../../components/buttons/generic-button";
import { UserDTO, UserService } from "../../core/user/user.service";

type TabId = 'account' | 'system';

interface UserForm {
    completeName: FormControl<string>;
    email: FormControl<string>;
    biography: FormControl<string>;
    birthDate: FormControl<string>;
    city: FormControl<string>;
}

@Component({
    selector: 'settings',
    imports: [Sidebar, GenericInput, ReactiveFormsModule, GenericButton],
    templateUrl: './settings.html',
})
export class Settings implements OnInit {
    private readonly userService = inject(UserService);

    readonly tabs: { id: TabId; label: string; icon: string; color: string }[] = [
        { id: 'account', label: 'Conta', icon: 'icons/user.svg', color: 'text-primary' },
        { id: 'system', label: 'Sistema', icon: 'icons/user.svg', color: 'text-primary' },
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
        this.form.patchValue({
            completeName: user.name,
            email: user.email,
            biography: user.description ?? '',
            birthDate: user.birthday?.slice(0, 10) ?? '',
            city: user.city ?? '',
        });

        this.form.markAsPristine();
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
}