import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup } from '@angular/forms';
import { applyFieldErrors } from './field-errors.util';

describe('applyFieldErrors', () => {
  const makeForm = () =>
    new FormGroup({ name: new FormControl(''), birthDate: new FormControl('') });

  const error = (body: unknown) => new HttpErrorResponse({ status: 400, error: body });

  it('retorna os erros gerais quando não há fieldErrors', () => {
    expect(applyFieldErrors(makeForm(), error({ errors: ['A', 'B'] }))).toBe('A B');
  });

  it('retorna mensagem padrão quando o corpo está vazio', () => {
    expect(applyFieldErrors(makeForm(), error(null))).toBe('Ocorreu um erro. Tente novamente.');
    expect(applyFieldErrors(makeForm(), error({ fieldErrors: {} }))).toBe(
      'Ocorreu um erro. Tente novamente.',
    );
  });

  it('aplica os erros nos controles correspondentes e retorna null', () => {
    const form = makeForm();

    const result = applyFieldErrors(form, error({ fieldErrors: { name: ['Informe seu nome.'] } }));

    expect(result).toBeNull();
    expect(form.controls.name.errors).toEqual({ backend: 'Informe seu nome.' });
    expect(form.controls.name.touched).toBe(true);
  });

  it('usa o mapa de nomes para traduzir campos do backend', () => {
    const form = makeForm();

    applyFieldErrors(form, error({ fieldErrors: { birthday: ['Data inválida'] } }), {
      birthday: 'birthDate',
    });

    expect(form.controls.birthDate.errors).toEqual({ backend: 'Data inválida' });
  });

  it('retorna mensagens de campos que não existem no formulário', () => {
    const form = makeForm();

    const result = applyFieldErrors(
      form,
      error({ fieldErrors: { email: ['E-mail já existe.'], name: ['Nome inválido'] } }),
    );

    expect(result).toBe('E-mail já existe.');
    expect(form.controls.name.errors).toEqual({ backend: 'Nome inválido' });
  });
});
