import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgClass } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
    catchError,
    debounceTime,
    distinctUntilChanged,
    finalize,
    from,
    of,
    switchMap,
    tap,
} from 'rxjs';

import { Sidebar } from '../../components/sidebar/sidebar';
import { GenericInput } from '../../components/input/generic-input/generic-input';
import { GenericButton } from '../../components/buttons/generic-button';
import { CategoryService } from '../../core/category/category.service';
import { CategoryModel } from '../../core/category/CategoryModel';
import { EventService } from '../../core/event/event.service';
import { CreateEventModel } from '../../core/event/event.model';
import { PlaceProvider, PlaceSuggestion } from '../../core/places/place-provider';
import { compressImage } from '../../core/utils/image-compressor';

interface NewEventForm {
    eventName: FormControl<string>;
    category: FormControl<string>;
    description: FormControl<string>;
    date: FormControl<string>;
    time: FormControl<string>;
    localization: FormControl<string>;
    city: FormControl<string>;
    spots: FormControl<number>;
    level: FormControl<string>;
}

@Component({
    selector: 'app-new-event',
    standalone: true,
    imports: [Sidebar, GenericInput, GenericButton, NgClass, ReactiveFormsModule],
    templateUrl: 'newEvent.html',
})
export class NewEvent implements OnInit {
    private readonly categoryService = inject(CategoryService);
    private readonly eventService = inject(EventService);
    private readonly places = inject(PlaceProvider);
    private readonly router = inject(Router);
    private readonly destroyRef = inject(DestroyRef);

    readonly categories = signal<CategoryModel[]>([]);
    readonly loading = signal(true);
    readonly categoryValue = signal<CategoryModel | null>(null);

    readonly selectedImage = signal<File | null>(null);
    readonly imagePreview = signal<string | null>(null);

    readonly suggestions = signal<PlaceSuggestion[]>([]);
    readonly placeSelected = signal(false);
    readonly localizationTyped = signal('');
    readonly searchFinished = signal(true);

    readonly showCityInput = computed(
        () =>
            this.localizationTyped().trim().length > 0 &&
            !this.placeSelected() &&
            this.searchFinished(),
    );

    private readonly allowedImageTypes = ['image/jpeg', 'image/png', 'image/webp'];
    private readonly maxOriginalSize = 25 * 1024 * 1024;
    private readonly maxImageSize = 5 * 1024 * 1024;

    readonly submitting = signal(false);
    readonly errorMessage = signal<string | null>(null);

    readonly form = new FormGroup<NewEventForm>({
        eventName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
        category: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
        description: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
        date: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
        time: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
        localization: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
        city: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
        spots: new FormControl(0, {
            nonNullable: true,
            validators: [Validators.required, Validators.min(1)],
        }),
        level: new FormControl('', { nonNullable: true }),
    });

    ngOnInit(): void {
        this.loadCategories();
        this.listenLocalizationChanges();
    }

    private loadCategories(): void {
        this.categoryService.getCategories().subscribe({
            next: (data) => {
                this.categories.set(data);
                this.loading.set(false);
            },
            error: (error) => {
                console.log(error);
                this.loading.set(false);
            },
        });
    }

    private listenLocalizationChanges(): void {
        this.form.controls.localization.valueChanges
            .pipe(
                tap((text) => {
                    if (this.placeSelected()) {
                        this.form.controls.city.setValue('');
                        this.placeSelected.set(false);
                    }
                    this.localizationTyped.set(text);
                    this.searchFinished.set(false);
                }),
                debounceTime(700),
                distinctUntilChanged(),
                switchMap((text) =>
                    text.trim().length < 4
                        ? of([] as PlaceSuggestion[])
                        : from(this.places.search(text)).pipe(
                              catchError(() => of([] as PlaceSuggestion[])),
                          ),
                ),
                takeUntilDestroyed(this.destroyRef),
            )
            .subscribe((list) => {
                this.suggestions.set(list);
                this.searchFinished.set(true);
            });
    }

    async selectPlace(suggestion: PlaceSuggestion): Promise<void> {
        const { localization, city } = await this.places.resolve(suggestion);

        this.form.controls.localization.setValue(localization, { emitEvent: false });
        this.form.controls.city.setValue(city);
        this.localizationTyped.set(localization);
        this.placeSelected.set(true);
        this.suggestions.set([]);
        this.errorMessage.set(null);
    }

    selectCategory(category: CategoryModel): void {
        this.categoryValue.set(category);
        this.form.controls.category.setValue(category.id);
    }

    async onImageSelected(event: Event): Promise<void> {
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0] ?? null;

        if (!file) {
            return;
        }

        if (!this.allowedImageTypes.includes(file.type)) {
            this.errorMessage.set('Formato inválido. Use JPG, PNG ou WEBP.');
            input.value = '';
            return;
        }

        if (file.size > this.maxOriginalSize) {
            this.errorMessage.set('A imagem é muito grande (máximo 25 MB).');
            input.value = '';
            return;
        }

        let finalFile: File;

        try {
            finalFile = await compressImage(file);
        } catch {
            this.errorMessage.set('Não foi possível processar essa imagem. Tente outra.');
            input.value = '';
            return;
        }

        if (finalFile.size > this.maxImageSize) {
            this.errorMessage.set('A imagem continua acima de 5 MB. Tente uma menor.');
            input.value = '';
            return;
        }

        this.errorMessage.set(null);

        const previousPreview = this.imagePreview();
        if (previousPreview) {
            URL.revokeObjectURL(previousPreview);
        }

        this.selectedImage.set(finalFile);
        this.imagePreview.set(URL.createObjectURL(finalFile));
    }

    private extractError(err: any): string {
        if (err.status === 413) {
            return 'A imagem deve ter no máximo 5 MB.';
        }

        const e = err.error;

        if (Array.isArray(e?.errors)) {
            return e.errors[0];
        }

        if (e?.errors && typeof e.errors === 'object') {
            const first = (Object.values(e.errors).flat() as string[])[0];
            if (first) return first;
        }

        return e?.title ?? 'Não foi possível criar o evento';
    }

    cancel(): void {
        this.router.navigate(['/events']);
    }

    submit(): void {
        if (this.submitting()) {
            return;
        }

        const image = this.selectedImage();

        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        if (!image) {
            this.errorMessage.set('Selecione uma imagem para o evento');
            return;
        }

        const v = this.form.getRawValue();

        this.submitting.set(true);
        this.errorMessage.set(null);

        this.eventService
            .uploadImage(image)
            .pipe(
                switchMap(({ url }) => {
                    const payload: CreateEventModel = {
                        title: v.eventName,
                        description: v.description,
                        dateTime: new Date(`${v.date}T${v.time}`).toISOString(),
                        localization: v.localization,
                        maxCapacity: v.spots,
                        city: v.city.trim(),
                        level: v.level || null,
                        urlImage: url,
                        categoryId: v.category,
                    };
                    return this.eventService.createEvent(payload);
                }),
                finalize(() => this.submitting.set(false)),
            )
            .subscribe({
                next: () => this.router.navigate(['/events']),
                error: (err) => {
                    console.error('Erro ao criar evento:', err.status, err.error);
                    this.errorMessage.set(this.extractError(err));
                },
            });
    }
}