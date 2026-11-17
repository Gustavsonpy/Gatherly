import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'warning' | 'info';
export interface Toast { id: number; type: ToastType; title: string; message: string; }

const DEFAULT_TITLES: Record<ToastType, string> = {
  success: 'Concluído',
  error: 'Erro',
  warning: 'Atenção',
  info: 'Informação',
};

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 0;
  readonly toasts = signal<Toast[]>([]);

  show(type: ToastType, message: string, title = DEFAULT_TITLES[type], duration = 4000) {
    const id = this.nextId++;
    this.toasts.update(list => [...list, { id, type, title, message }]);
    setTimeout(() => this.dismiss(id), duration);
  }

  success(msg: string, title?: string) { this.show('success', msg, title); }
  error(msg: string, title?: string)   { this.show('error', msg, title, 6000); }
  warning(msg: string, title?: string) { this.show('warning', msg, title); }
  info(msg: string, title?: string)    { this.show('info', msg, title); }

  dismiss(id: number) {
    this.toasts.update(list => list.filter(t => t.id !== id));
  }
}
