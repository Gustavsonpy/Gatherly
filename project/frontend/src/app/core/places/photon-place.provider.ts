import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { PlaceProvider, PlaceSuggestion, SelectedPlace } from './place-provider';

/**
 * Photon (https://photon.komoot.io): geocoder grátis baseado nos dados do OpenStreetMap,
 * feito para autocomplete ("busca enquanto digita"), ao contrário do Nominatim.
 */
interface PhotonFeature {
    geometry: { coordinates: [number, number] }; // [longitude, latitude]
    properties: {
        osm_type?: string;
        osm_id?: number;
        name?: string;
        street?: string;
        housenumber?: string;
        district?: string;
        locality?: string;
        city?: string;
        county?: string;
        state?: string;
        countrycode?: string;
    };
}

interface PhotonResponse {
    features: PhotonFeature[];
}

// Retângulo que cobre o Brasil (minLon, minLat, maxLon, maxLat)
const BRAZIL_BBOX = '-74.0,-34.0,-34.7,5.3';

const UF_BY_STATE: Record<string, string> = {
    acre: 'AC', alagoas: 'AL', amapa: 'AP', amazonas: 'AM', bahia: 'BA', ceara: 'CE',
    'distrito federal': 'DF', 'espirito santo': 'ES', goias: 'GO', maranhao: 'MA',
    'mato grosso': 'MT', 'mato grosso do sul': 'MS', 'minas gerais': 'MG', para: 'PA',
    paraiba: 'PB', parana: 'PR', pernambuco: 'PE', piaui: 'PI', 'rio de janeiro': 'RJ',
    'rio grande do norte': 'RN', 'rio grande do sul': 'RS', rondonia: 'RO', roraima: 'RR',
    'santa catarina': 'SC', 'sao paulo': 'SP', sergipe: 'SE', tocantins: 'TO',
};

function toUf(state?: string): string | undefined {
    if (!state) return undefined;
    const key = state.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase().trim();
    return UF_BY_STATE[key];
}

@Injectable()
export class PhotonPlaceProvider extends PlaceProvider {
    private readonly http = inject(HttpClient);

    async search(text: string): Promise<PlaceSuggestion[]> {
        const res = await firstValueFrom(
            this.http.get<PhotonResponse>('https://photon.komoot.io/api/', {
                params: { q: text, limit: '5', bbox: BRAZIL_BBOX },
            }),
        );

        return res.features
            .filter((f) => f.properties.countrycode === 'BR')
            .map((f, i) => {
                const p = f.properties;
                const [longitude, latitude] = f.geometry.coordinates;
                const city = p.city ?? p.locality ?? p.county ?? '';
                const district = p.district ?? (p.locality !== city ? p.locality : undefined);
                const state = toUf(p.state);
                const street = [p.street, p.housenumber].filter(Boolean).join(', ') || undefined;
                const placeName = p.name || street;

                const cityState = [city, state].filter(Boolean).join(' - ');
                const label = [...new Set([placeName, street, district, cityState].filter(Boolean))].join(', ');

                return {
                    id: `${p.osm_type ?? ''}${p.osm_id ?? i}`,
                    label,
                    city,
                    placeName,
                    street,
                    district,
                    state,
                    latitude,
                    longitude,
                };
            });
    }

    async resolve(s: PlaceSuggestion): Promise<SelectedPlace> {
        const { id, label, city, ...details } = s;
        return { ...details, localization: label, city: city ?? '' };
    }
}
