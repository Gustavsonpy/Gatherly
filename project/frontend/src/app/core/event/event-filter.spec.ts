import { describe, expect, it } from 'vitest';
import { EventModel } from './event.model';
import { EMPTY_EVENT_FILTERS, EventFilters, cityLabel, filterEvents, hasActiveFilters } from './event-filter';

function makeEvent(overrides: Partial<EventModel>): EventModel {
    return {
        id: 'id',
        title: 'Evento',
        description: '',
        dateTimeStart: '2026-12-10T15:00:00',
        dateTimeEnd: '2026-12-10T17:00:00',
        localization: 'Rua A',
        maxCapacity: 10,
        city: 'Joinville',
        urlImage: '',
        registerDate: '',
        userId: 'u1',
        categoryId: 'esportes',
        categoryName: 'Esportes',
        participantsCount: 0,
        isParticipating: false,
        creatorName: null,
        creatorPhoto: null,
        status: 'upcoming',
        ...overrides,
    };
}

const filters = (f: Partial<EventFilters>): EventFilters => ({ ...EMPTY_EVENT_FILTERS, ...f });

const futebol = makeEvent({ id: '1', title: 'Futebol no parque', city: 'São José', maxCapacity: 14, participantsCount: 12 });
const xadrez = makeEvent({ id: '2', title: 'Torneio de xadrez', categoryId: 'jogos', categoryName: 'Jogos', dateTimeStart: '2026-12-20T19:00:00' });
const show = makeEvent({ id: '3', title: 'Show acústico', categoryId: 'musica', categoryName: 'Música', maxCapacity: 50, participantsCount: 10 });
const all = [futebol, xadrez, show];

const ids = (list: EventModel[]) => list.map((e) => e.id);

describe('filterEvents', () => {
    it('sem filtros devolve todos os eventos', () => {
        expect(ids(filterEvents(all, EMPTY_EVENT_FILTERS))).toEqual(['1', '2', '3']);
    });

    it('busca por texto ignorando acentos e maiúsculas', () => {
        expect(ids(filterEvents(all, filters({ query: 'ACUSTICO' })))).toEqual(['3']);
        expect(ids(filterEvents(all, filters({ query: 'sao jose' })))).toEqual(['1']);
    });

    it('busca também pelo nome da categoria', () => {
        expect(ids(filterEvents(all, filters({ query: 'musica' })))).toEqual(['3']);
    });

    it('filtra por categoria e cidade', () => {
        expect(ids(filterEvents(all, filters({ categoryId: 'jogos' })))).toEqual(['2']);
        expect(ids(filterEvents(all, filters({ city: 'Joinville' })))).toEqual(['2', '3']);
    });

    it('diferencia cidades com o mesmo nome em estados diferentes', () => {
        const noRS = makeEvent({ id: 'rs', city: 'Bom Jesus', state: 'RS' });
        const noPI = makeEvent({ id: 'pi', city: 'Bom Jesus', state: 'PI' });

        expect(ids(filterEvents([noRS, noPI], filters({ city: 'Bom Jesus - RS' })))).toEqual(['rs']);
    });

    it('filtra por intervalo de datas (inclusivo)', () => {
        expect(ids(filterEvents(all, filters({ dateFrom: '2026-12-15' })))).toEqual(['2']);
        expect(ids(filterEvents(all, filters({ dateTo: '2026-12-10' })))).toEqual(['1', '3']);
        expect(ids(filterEvents(all, filters({ dateFrom: '2026-12-10', dateTo: '2026-12-10' })))).toEqual(['1', '3']);
    });

    it('filtra por vagas livres mínimas', () => {
        expect(ids(filterEvents(all, filters({ minSpots: 3 })))).toEqual(['2', '3']);
        expect(ids(filterEvents(all, filters({ minSpots: 20 })))).toEqual(['3']);
    });

    it('combina filtros', () => {
        expect(ids(filterEvents(all, filters({ city: 'Joinville', minSpots: 20 })))).toEqual(['3']);
    });
});

describe('cityLabel', () => {
    it('junta cidade e UF quando o evento tem estado', () => {
        expect(cityLabel(makeEvent({ city: 'Joinville', state: 'SC' }))).toBe('Joinville - SC');
    });

    it('usa só a cidade quando o evento não tem estado', () => {
        expect(cityLabel(makeEvent({ city: 'Joinville' }))).toBe('Joinville');
    });
});

describe('hasActiveFilters', () => {
    it('detecta quando há algum filtro ativo', () => {
        expect(hasActiveFilters(EMPTY_EVENT_FILTERS)).toBe(false);
        expect(hasActiveFilters(filters({ query: '   ' }))).toBe(false);
        expect(hasActiveFilters(filters({ minSpots: 1 }))).toBe(true);
    });
});
