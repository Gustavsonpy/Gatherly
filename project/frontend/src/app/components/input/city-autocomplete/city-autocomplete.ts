import { Component, computed, DestroyRef, inject, input, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl } from '@angular/forms';
import { CityOption, IbgeCityService, normalizeText } from '../../../core/places/ibge-city.service';

const MIN_CHARS = 2;
const MAX_SUGGESTIONS = 8;

@Component({
    selector: 'city-autocomplete',
    standalone: true,
    template: `
        <div class="flex flex-col">
            <label [for]="inputId()">{{ label() }}</label>

            <div class="relative">
                <input
                    [id]="inputId()"
                    type="text"
                    role="combobox"
                    autocomplete="off"
                    aria-autocomplete="list"
                    [placeholder]="placeholder()"
                    [value]="query()"
                    [attr.aria-expanded]="showList()"
                    [attr.aria-controls]="listId()"
                    [attr.aria-activedescendant]="activeDescendant()"
                    class="border border-primary-dark rounded-md p-2 text-[12px] w-full"
                    (focus)="ensureLoaded()"
                    (input)="onInput($event)"
                    (keydown)="onKeydown($event)"
                    (blur)="onBlur()"
                />

                @if (showList()) {
                    <ul
                        [id]="listId()"
                        role="listbox"
                        class="absolute left-0 right-0 top-full mt-1 z-10 max-h-48 overflow-auto bg-white
                               border border-primary-dark rounded-sm shadow"
                    >
                        @if (loading()) {
                            <li class="px-2 py-1.5 text-xs text-gray-400">Carregando cidades...</li>
                        } @else if (suggestions().length === 0) {
                            <li class="px-2 py-1.5 text-xs text-gray-400">Nenhuma cidade encontrada</li>
                        } @else {
                            @for (option of suggestions(); track option.label; let i = $index) {
                                <li
                                    role="option"
                                    [id]="optionId(i)"
                                    [attr.aria-selected]="i === activeIndex()"
                                    class="px-2 py-1.5 text-xs cursor-pointer hover:bg-primary-light"
                                    [class.bg-primary-light]="i === activeIndex()"
                                    (mousedown)="$event.preventDefault(); select(option)"
                                >
                                    {{ option.label }}
                                </li>
                            }
                        }
                    </ul>
                }
            </div>

            <p class="text-[12px] text-red-500" [class.invisible]="!loadError()">
                Não foi possível carregar a lista de cidades. Tente novamente.
            </p>
        </div>
    `,
})
export class CityAutocomplete implements OnInit {
    private readonly cityService = inject(IbgeCityService);
    private readonly destroyRef = inject(DestroyRef);

    readonly inputId = input.required<string>();
    readonly label = input.required<string>();
    readonly control = input.required<FormControl<string>>();
    readonly placeholder = input<string>('');

    readonly query = signal('');
    readonly open = signal(false);
    readonly loading = signal(false);
    readonly loadError = signal(false);
    readonly activeIndex = signal(0);

    private readonly cities = signal<CityOption[]>([]);

    readonly listId = computed(() => `${this.inputId()}-listbox`);

    readonly showList = computed(
        () => this.open() && !this.loadError() && normalizeText(this.query()).length >= MIN_CHARS,
    );

    readonly suggestions = computed(() => {
        const q = normalizeText(this.query());

        if (q.length < MIN_CHARS) {
            return [];
        }

        const startsWith: CityOption[] = [];
        const contains: CityOption[] = [];

        for (const city of this.cities()) {
            if (city.key.startsWith(q)) {
                startsWith.push(city);
            } else if (city.key.includes(q)) {
                contains.push(city);
            }

            if (startsWith.length >= MAX_SUGGESTIONS) {
                break;
            }
        }

        return [...startsWith, ...contains].slice(0, MAX_SUGGESTIONS);
    });

    readonly activeDescendant = computed(() =>
        this.showList() && !this.loading() && this.suggestions().length
            ? this.optionId(this.activeIndex())
            : null,
    );

    ngOnInit(): void {
        const control = this.control();

        this.query.set(control.value ?? '');

        control.valueChanges
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe((value) => this.query.set(value ?? ''));
    }

    optionId(index: number): string {
        return `${this.inputId()}-option-${index}`;
    }

    ensureLoaded(): void {
        if (this.cities().length > 0 || this.loading()) {
            return;
        }

        this.loading.set(true);
        this.loadError.set(false);

        this.cityService
            .load()
            .then((list) => {
                this.cities.set(list);
                this.loading.set(false);
            })
            .catch((error) => {
                console.error(error);
                this.loading.set(false);
                this.loadError.set(true);
            });
    }

    onInput(event: Event): void {
        this.query.set((event.target as HTMLInputElement).value);
        this.activeIndex.set(0);
        this.open.set(true);
        this.ensureLoaded();
    }

    onKeydown(event: KeyboardEvent): void {
        const total = this.suggestions().length;

        switch (event.key) {
            case 'ArrowDown':
                event.preventDefault();
                this.open.set(true);
                this.activeIndex.update((i) => Math.min(i + 1, Math.max(total - 1, 0)));
                break;
            case 'ArrowUp':
                event.preventDefault();
                this.activeIndex.update((i) => Math.max(i - 1, 0));
                break;
            case 'Enter': {
                const option = this.suggestions()[this.activeIndex()];
                if (this.showList() && option) {
                    event.preventDefault();
                    this.select(option);
                }
                break;
            }
            case 'Escape':
                this.open.set(false);
                break;
        }
    }

    select(option: CityOption): void {
        this.commit(option.label);
        this.open.set(false);
        this.activeIndex.set(0);
    }

    onBlur(): void {
        const control = this.control();
        const text = this.query().trim();

        this.open.set(false);
        control.markAsTouched();

        if (!text) {
            this.commit('');
            return;
        }

        if (text === control.value) {
            return;
        }

        const match = this.findExact(text);

        if (match) {
            this.commit(match.label);
        } else {
            this.query.set(control.value);
        }
    }

    private findExact(text: string): CityOption | undefined {
        const key = normalizeText(text);
        const all = this.cities();

        const byLabel = all.find((city) => city.key === key);

        if (byLabel) {
            return byLabel;
        }

        const byName = all.filter((city) => city.nameKey === key);

        return byName.length === 1 ? byName[0] : undefined;
    }

    private commit(value: string): void {
        const control = this.control();

        if (control.value !== value) {
            control.setValue(value);
            control.markAsDirty();
        }

        this.query.set(value);
    }
}