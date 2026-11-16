import { inject, Injectable } from '@angular/core';
import { PlaceProvider, PlaceSuggestion, SelectedPlace } from './place-provider';
import { PhotonPlaceProvider } from './photon-place.provider';
import { NominatimPlaceProvider } from './nominatim-place.provider';

const PRIMARY_TIMEOUT_MS = 4000;

/**
 * Usa o Photon como principal. Se ele falhar ou demorar mais que 4s,
 * tenta o Nominatim. As chamadas saem do navegador do usuário, não do nosso servidor.
 */
@Injectable()
export class FallbackPlaceProvider extends PlaceProvider {
    private readonly primary = inject(PhotonPlaceProvider);
    private readonly secondary = inject(NominatimPlaceProvider);

    // Lembra qual provider gerou cada sugestão, para o resolve() usar o mesmo.
    private readonly origin = new WeakMap<PlaceSuggestion, PlaceProvider>();

    async search(text: string): Promise<PlaceSuggestion[]> {
        let results: PlaceSuggestion[];
        let provider: PlaceProvider = this.primary;

        try {
            results = await withTimeout(this.primary.search(text), PRIMARY_TIMEOUT_MS);
        } catch (err) {
            console.warn('Photon indisponível, usando Nominatim.', err);
            provider = this.secondary;
            results = await this.secondary.search(text);
        }

        results.forEach((s) => this.origin.set(s, provider));
        return results;
    }

    resolve(suggestion: PlaceSuggestion): Promise<SelectedPlace> {
        return (this.origin.get(suggestion) ?? this.primary).resolve(suggestion);
    }
}

function withTimeout<T>(promise: Promise<T>, ms: number): Promise<T> {
    return new Promise<T>((resolve, reject) => {
        const timer = setTimeout(() => reject(new Error(`timeout ${ms}ms`)), ms);
        promise.then(
            (v) => { clearTimeout(timer); resolve(v); },
            (e) => { clearTimeout(timer); reject(e); },
        );
    });
}
