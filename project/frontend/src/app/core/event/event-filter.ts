import { EventModel } from './event.model';

export interface EventFilters {
    query: string;
    categoryId: string | null;
    city: string | null;
    dateFrom: string | null;
    dateTo: string | null;
    minSpots: number;
}

export const EMPTY_EVENT_FILTERS: EventFilters = {
    query: '',
    categoryId: null,
    city: null,
    dateFrom: null,
    dateTo: null,
    minSpots: 0,
};

export function normalizeText(value: string | null | undefined): string {
    return (value ?? '')
        .normalize('NFD')
        .replace(/[̀-ͯ]/g, '')
        .toLowerCase()
        .trim();
}

/** "Joinville - SC" (mesmo formato do IBGE), ou só "Joinville" se o evento não tiver UF. */
export function cityLabel(event: EventModel): string {
    return event.state ? `${event.city} - ${event.state}` : event.city;
}

export function availableSpots(event: EventModel): number {
    return Math.max(event.maxCapacity - event.participantsCount, 0);
}

export function hasActiveFilters(filters: EventFilters): boolean {
    return (
        filters.query.trim() !== '' ||
        filters.categoryId !== null ||
        filters.city !== null ||
        filters.dateFrom !== null ||
        filters.dateTo !== null ||
        filters.minSpots > 0
    );
}

function localDateKey(iso: string): string {
    const d = new Date(iso);
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

export function filterEvents(events: EventModel[], filters: EventFilters): EventModel[] {
    const query = normalizeText(filters.query);
    const city = normalizeText(filters.city);

    return events.filter((e) => {
        if (query) {
            const haystack = normalizeText(`${e.title} ${e.localization} ${e.city} ${e.categoryName}`);
            if (!haystack.includes(query)) return false;
        }

        if (filters.categoryId && e.categoryId !== filters.categoryId) return false;

        if (city && normalizeText(cityLabel(e)) !== city) return false;

        const day = localDateKey(e.dateTimeStart);
        if (filters.dateFrom && day < filters.dateFrom) return false;
        if (filters.dateTo && day > filters.dateTo) return false;

        if (filters.minSpots > 0 && availableSpots(e) < filters.minSpots) return false;

        return true;
    });
}
