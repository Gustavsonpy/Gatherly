import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { PlaceProvider, PlaceSuggestion, SelectedPlace } from './place-provider';

interface NominatimResult {
    place_id: number;
    lat: string;
    lon: string;
    name?: string;
    display_name: string;
    address: {
        road?: string; house_number?: string; suburb?: string;
        city?: string; town?: string; village?: string; municipality?: string; county?: string;
        state?: string; 'ISO3166-2-lvl4'?: string;
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
            placeName: r.name || r.address.road,
            street: [r.address.road, r.address.house_number].filter(Boolean).join(', '),
            district: r.address.suburb,
            state: r.address['ISO3166-2-lvl4']?.replace('BR-', ''),
            latitude: Number(r.lat),
            longitude: Number(r.lon),
        }));
    }

    async resolve(s: PlaceSuggestion): Promise<SelectedPlace> {
        const { id, label, city, ...details } = s;
        return { ...details, localization: label, city: city ?? '' };
    }
}