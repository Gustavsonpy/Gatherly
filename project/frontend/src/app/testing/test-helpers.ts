import { EventModel } from '../core/event/event.model';

/**
 * Substitui o localStorage global por uma implementação em memória.
 * Retorna o Map usado internamente para inspeção nos testes.
 */
export function stubLocalStorage(initial: Record<string, string> = {}): Map<string, string> {
  const store = new Map<string, string>(Object.entries(initial));

  vi.stubGlobal('localStorage', {
    getItem: (key: string) => store.get(key) ?? null,
    setItem: (key: string, value: string) => store.set(key, String(value)),
    removeItem: (key: string) => store.delete(key),
    clear: () => store.clear(),
  });

  return store;
}

/** Gera um JWT (sem assinatura válida) com as claims usadas pelo backend .NET. */
export function makeJwt(claims: { id?: string; email?: string; name?: string } = {}): string {
  const payload = {
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': claims.id ?? 'user-1',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress':
      claims.email ?? 'user@email.com',
    name: claims.name ?? 'Usuário Teste',
  };

  const base64Url = (obj: unknown) =>
    btoa(JSON.stringify(obj)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');

  return `${base64Url({ alg: 'HS256', typ: 'JWT' })}.${base64Url(payload)}.signature`;
}

export function makeEvent(overrides: Partial<EventModel> = {}): EventModel {
  return {
    id: 'event-1',
    title: 'Futebol no parque',
    description: 'Partida amistosa',
    dateTimeStart: '2030-01-10T18:00:00Z',
    dateTimeEnd: '2030-01-10T20:00:00Z',
    localization: 'Parque Barigui',
    maxCapacity: 10,
    city: 'Curitiba',
    urlImage: 'http://localhost:5008/uploads/events/img.jpg',
    registerDate: '2029-12-01T00:00:00Z',
    userId: 'user-1',
    categoryId: 'cat-1',
    categoryName: 'Esportes',
    participantsCount: 1,
    isParticipating: true,
    creatorName: 'Usuário Teste',
    creatorPhoto: null,
    status: 'upcoming',
    ...overrides,
  };
}
