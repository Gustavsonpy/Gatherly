import { Component, inject } from '@angular/core';
import { ToastService, ToastType } from '../../core/toaster/toaster.service';

@Component({
  selector: 'app-toaster',
  template: `
    <div class="fixed bottom-4 right-4 z-50 flex flex-col gap-4 w-80"
         aria-live="polite">
      @for (t of toast.toasts(); track t.id) {
        <div [attr.role]="t.type === 'error' ? 'alert' : 'status'"
             class="cursor-pointer rounded-2xl px-4 py-3 shadow-sm animate-[slide-in_.2s_ease-out]"
             [class]="styles[t.type].box"
             title="Clique para fechar"
             (click)="toast.dismiss(t.id)">
          <p class="text-[15px] font-bold" [class]="styles[t.type].title">{{ t.title }}</p>
          <p class="text-[12px]" [class]="styles[t.type].message">{{ t.message }}</p>
        </div>
      }
    </div>
  `,
})
export class ToasterComponent {
  toast = inject(ToastService);
  styles: Record<ToastType, { box: string; title: string; message: string }> = {
    success: { box: 'bg-green-100', title: 'text-green-800', message: 'text-green-700' },
    error:   { box: 'bg-red-200',   title: 'text-red-600',   message: 'text-red-600' },
    warning: { box: 'bg-amber-100', title: 'text-amber-800', message: 'text-amber-700' },
    info:    { box: 'bg-blue-100',  title: 'text-blue-800',  message: 'text-blue-700' },
  };
}
