export interface PlaceSuggestion {
    id: string;
    label: string;
    city?: string;
}

export interface SelectedPlace {
    localization: string;
    city: string;
}

export abstract class PlaceProvider {
    abstract search(text: string): Promise<PlaceSuggestion[]>;
    abstract resolve(suggestion: PlaceSuggestion): Promise<SelectedPlace>;
}