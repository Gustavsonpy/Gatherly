import { HttpErrorResponse } from '@angular/common/http';
import { getErrorMessage } from './http-error';

describe('getErrorMessage', () => {
  it('retorna o fallback quando não é HttpErrorResponse', () => {
    expect(getErrorMessage(new Error('x'))).toBe('Ocorreu um erro. Tente novamente.');
    expect(getErrorMessage('qualquer', 'Falhou')).toBe('Falhou');
  });

  it('retorna mensagem de conexão quando status é 0', () => {
    const err = new HttpErrorResponse({ status: 0 });
    expect(getErrorMessage(err)).toContain('Não foi possível conectar ao servidor');
  });

  it('junta os erros retornados pelo backend', () => {
    const err = new HttpErrorResponse({ status: 400, error: { errors: ['Erro 1.', 'Erro 2.'] } });
    expect(getErrorMessage(err)).toBe('Erro 1. Erro 2.');
  });

  it('retorna o fallback quando a lista de erros está vazia ou ausente', () => {
    expect(getErrorMessage(new HttpErrorResponse({ status: 400, error: { errors: [] } }), 'fb')).toBe('fb');
    expect(getErrorMessage(new HttpErrorResponse({ status: 500 }), 'fb')).toBe('fb');
  });
});
