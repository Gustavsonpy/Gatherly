import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { IbgeCityService, normalizeText } from './ibge-city.service';
import { PhotonPlaceProvider } from './photon-place.provider';
import { NominatimPlaceProvider } from './nominatim-place.provider';
import { FallbackPlaceProvider } from './fallback-place.provider';
import { PlaceSuggestion } from './place-provider';

describe('places', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        PhotonPlaceProvider,
        NominatimPlaceProvider,
        FallbackPlaceProvider,
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  describe('normalizeText', () => {
    it('remove acentos, espaços extras e converte para minúsculas', () => {
      expect(normalizeText('  São   Paulo ')).toBe('sao paulo');
      expect(normalizeText('Maringá - PR')).toBe('maringa - pr');
    });
  });

  describe('IbgeCityService', () => {
    const URL = 'https://servicodados.ibge.gov.br/api/v1/localidades/municipios';

    it('carrega, mapeia a UF e ordena as cidades', async () => {
      const service = TestBed.inject(IbgeCityService);
      const promise = service.load();

      http.expectOne(URL).flush([
        { id: 1, nome: 'Curitiba', microrregiao: { mesorregiao: { UF: { sigla: 'PR' } } } },
        {
          id: 2,
          nome: 'Boa Vista',
          microrregiao: null,
          'regiao-imediata': { 'regiao-intermediaria': { UF: { sigla: 'RR' } } },
        },
        { id: 3, nome: 'Água Doce' },
      ]);

      const result = await promise;
      expect(result.map((c) => c.label)).toEqual(['Água Doce', 'Boa Vista - RR', 'Curitiba - PR']);
      expect(result[2]).toEqual({ label: 'Curitiba - PR', key: 'curitiba - pr', nameKey: 'curitiba' });
    });

    it('usa cache em chamadas seguintes', async () => {
      const service = TestBed.inject(IbgeCityService);
      const first = service.load();
      http.expectOne(URL).flush([]);
      await first;

      await service.load();
      http.expectNone(URL);
    });

    it('limpa o cache quando a requisição falha', async () => {
      const service = TestBed.inject(IbgeCityService);
      const failing = service.load();
      http.expectOne(URL).flush(null, { status: 500, statusText: 'Erro' });
      await expect(failing).rejects.toBeTruthy();

      const retry = service.load();
      http.expectOne(URL).flush([]);
      await expect(retry).resolves.toEqual([]);
    });
  });

  describe('PhotonPlaceProvider', () => {
    it('filtra resultados fora do Brasil e monta as sugestões', async () => {
      const provider = TestBed.inject(PhotonPlaceProvider);
      const promise = provider.search('parque');

      const req = http.expectOne((r) => r.url === 'https://photon.komoot.io/api/');
      expect(req.request.params.get('q')).toBe('parque');
      req.flush({
        features: [
          {
            geometry: { coordinates: [-49.3, -25.4] },
            properties: {
              osm_type: 'W',
              osm_id: 10,
              name: 'Parque Barigui',
              street: 'Rua A',
              housenumber: '100',
              city: 'Curitiba',
              state: 'Paraná',
              countrycode: 'BR',
            },
          },
          {
            geometry: { coordinates: [-46.6, -23.5] },
            properties: { locality: 'Centro', county: 'São Paulo', state: 'Estado X', countrycode: 'BR' },
          },
          { geometry: { coordinates: [0, 0] }, properties: { name: 'Paris', countrycode: 'FR' } },
        ],
      });

      const [first, second, ...rest] = await promise;
      expect(rest).toEqual([]);
      expect(first).toEqual({
        id: 'W10',
        label: 'Parque Barigui, Rua A, 100, Curitiba - PR',
        city: 'Curitiba',
        placeName: 'Parque Barigui',
        street: 'Rua A, 100',
        district: undefined,
        state: 'PR',
        latitude: -25.4,
        longitude: -49.3,
      });
      expect(second.id).toBe('1');
      expect(second.city).toBe('Centro');
      expect(second.state).toBeUndefined();
    });

    it('resolve converte sugestão em local selecionado', async () => {
      const provider = TestBed.inject(PhotonPlaceProvider);
      const selected = await provider.resolve({ id: '1', label: 'Rua A', street: 'Rua A' });
      expect(selected).toEqual({ street: 'Rua A', localization: 'Rua A', city: '' });
    });
  });

  describe('NominatimPlaceProvider', () => {
    it('mapeia os resultados do Nominatim', async () => {
      const provider = TestBed.inject(NominatimPlaceProvider);
      const promise = provider.search('rua');

      http.expectOne((r) => r.url === 'https://nominatim.openstreetmap.org/search').flush([
        {
          place_id: 7,
          lat: '-25.1',
          lon: '-49.2',
          display_name: 'Rua B, Curitiba',
          address: {
            road: 'Rua B',
            house_number: '5',
            suburb: 'Centro',
            town: 'Curitiba',
            'ISO3166-2-lvl4': 'BR-PR',
          },
        },
        { place_id: 8, lat: '0', lon: '0', display_name: 'Algum lugar', address: {} },
      ]);

      const [first, second] = await promise;
      expect(first).toEqual({
        id: '7',
        label: 'Rua B, Curitiba',
        city: 'Curitiba',
        placeName: 'Rua B',
        street: 'Rua B, 5',
        district: 'Centro',
        state: 'PR',
        latitude: -25.1,
        longitude: -49.2,
      });
      expect(second.city).toBe('');
      expect(second.street).toBe('');
    });

    it('resolve converte sugestão em local selecionado', async () => {
      const provider = TestBed.inject(NominatimPlaceProvider);
      expect(await provider.resolve({ id: '1', label: 'X', city: 'Curitiba' })).toEqual({
        localization: 'X',
        city: 'Curitiba',
      });
    });
  });

  describe('FallbackPlaceProvider', () => {
    const suggestion: PlaceSuggestion = { id: '1', label: 'Lugar' };

    it('usa o Photon quando ele responde', async () => {
      const photon = TestBed.inject(PhotonPlaceProvider);
      const nominatim = TestBed.inject(NominatimPlaceProvider);
      vi.spyOn(photon, 'search').mockResolvedValue([suggestion]);
      const nominatimSearch = vi.spyOn(nominatim, 'search');
      const photonResolve = vi.spyOn(photon, 'resolve').mockResolvedValue({ localization: 'L', city: 'C' });

      const fallback = TestBed.inject(FallbackPlaceProvider);
      expect(await fallback.search('x')).toEqual([suggestion]);
      await fallback.resolve(suggestion);

      expect(nominatimSearch).not.toHaveBeenCalled();
      expect(photonResolve).toHaveBeenCalledWith(suggestion);
    });

    it('usa o Nominatim quando o Photon falha e resolve com o mesmo provider', async () => {
      vi.spyOn(console, 'warn').mockImplementation(() => {});
      const photon = TestBed.inject(PhotonPlaceProvider);
      const nominatim = TestBed.inject(NominatimPlaceProvider);
      vi.spyOn(photon, 'search').mockRejectedValue(new Error('offline'));
      vi.spyOn(nominatim, 'search').mockResolvedValue([suggestion]);
      const nominatimResolve = vi
        .spyOn(nominatim, 'resolve')
        .mockResolvedValue({ localization: 'L', city: 'C' });

      const fallback = TestBed.inject(FallbackPlaceProvider);
      expect(await fallback.search('x')).toEqual([suggestion]);
      await fallback.resolve(suggestion);

      expect(nominatimResolve).toHaveBeenCalledWith(suggestion);
    });

    it('usa o Nominatim quando o Photon demora mais de 4s', async () => {
      vi.useFakeTimers();
      vi.spyOn(console, 'warn').mockImplementation(() => {});
      const photon = TestBed.inject(PhotonPlaceProvider);
      const nominatim = TestBed.inject(NominatimPlaceProvider);
      vi.spyOn(photon, 'search').mockReturnValue(new Promise(() => {}));
      vi.spyOn(nominatim, 'search').mockResolvedValue([suggestion]);

      const promise = TestBed.inject(FallbackPlaceProvider).search('x');
      await vi.advanceTimersByTimeAsync(4000);

      expect(await promise).toEqual([suggestion]);
      vi.useRealTimers();
    });

    it('resolve usa o Photon para sugestões desconhecidas', async () => {
      const photon = TestBed.inject(PhotonPlaceProvider);
      const resolve = vi.spyOn(photon, 'resolve').mockResolvedValue({ localization: 'L', city: 'C' });

      await TestBed.inject(FallbackPlaceProvider).resolve(suggestion);

      expect(resolve).toHaveBeenCalled();
    });
  });
});
