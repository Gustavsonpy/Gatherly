import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const IBGE_MUNICIPIOS_URL = 'https://servicodados.ibge.gov.br/api/v1/localidades/municipios';

interface IbgeUF {
    sigla?: string;
}

interface IbgeMunicipio {
    id: number;
    nome: string;
    microrregiao?: { mesorregiao?: { UF?: IbgeUF } } | null;
    'regiao-imediata'?: { 'regiao-intermediaria'?: { UF?: IbgeUF } } | null;
}

export interface CityOption {
    label: string;
    key: string;
    nameKey: string;
}

export function normalizeText(text: string): string {
    return text
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .toLowerCase()
        .replace(/\s+/g, ' ')
        .trim();
}

function toOption(m: IbgeMunicipio): CityOption {
    const uf =
        m.microrregiao?.mesorregiao?.UF?.sigla ??
        m['regiao-imediata']?.['regiao-intermediaria']?.UF?.sigla;

    const label = uf ? `${m.nome} - ${uf}` : m.nome;

    return { label, key: normalizeText(label), nameKey: normalizeText(m.nome) };
}

@Injectable({ providedIn: 'root' })
export class IbgeCityService {
    private readonly http = inject(HttpClient);
    private cache?: Promise<CityOption[]>;

    load(): Promise<CityOption[]> {
        if (!this.cache) {
            this.cache = firstValueFrom(this.http.get<IbgeMunicipio[]>(IBGE_MUNICIPIOS_URL))
                .then((list) =>
                    list.map(toOption).sort((a, b) => a.label.localeCompare(b.label, 'pt-BR')),
                )
                .catch((error) => {
                    this.cache = undefined;
                    throw error;
                });
        }

        return this.cache;
    }
}