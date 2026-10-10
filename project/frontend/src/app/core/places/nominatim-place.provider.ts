import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { PlaceProvider, PlaceSuggestion, SelectedPlace } from './place-provider';

interface NominatimResult {
    place_id: number;
    display_name: string;
    address: {
        city?: string;
        town?: string;
        village?: string;
        municipality?: string;
        county?: string;
    };
}

@Injectable()
export class NominatimPlaceProvider extends PlaceProvider {
    private readonly http = inject(HttpClient);

    async search(text: string): Promise<PlaceSuggestion[]> {
        const results = await firstValueFrom(
            this.http.get<NominatimResult[]>('https://nominatim.openstreetmap.org/search', {
                params: {
                    q: text,
                    format: 'jsonv2',
                    addressdetails: '1',
                    countrycodes: 'br',
                    limit: '5',
                    'accept-language': 'pt-BR',
                },
            }),
        );

        return results.map((r) => ({
            id: String(r.place_id),
            label: r.display_name,
            city:
                r.address.city ??
                r.address.town ??
                r.address.village ??
                r.address.municipality ??
                r.address.county ??
                '',
        }));
    }

    async resolve(s: PlaceSuggestion): Promise<SelectedPlace> {
        return { localization: s.label, city: s.city ?? '' };
    }
}