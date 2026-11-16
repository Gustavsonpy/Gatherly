export interface PlaceDetails {
    placeName?: string;
    street?: string;
    district?: string;
    state?: string;
    latitude?: number;
    longitude?: number;
}

export interface PlaceSuggestion extends PlaceDetails {
    id: string;
    label: string;
    city?: string;
}

export interface SelectedPlace extends PlaceDetails {
    localization: string;
    city: string;
}

export abstract class PlaceProvider {
    abstract search(text: string): Promise<PlaceSuggestion[]>;
    abstract resolve(suggestion: PlaceSuggestion): Promise<SelectedPlace>;
}
