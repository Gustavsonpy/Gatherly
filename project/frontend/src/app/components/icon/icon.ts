import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-icon',
  template: '',
  styles: `
    :host {
      display: inline-block;
      flex-shrink: 0;
      background-color: currentColor;
      -webkit-mask: var(--icon-url) center / contain no-repeat;
      mask: var(--icon-url) center / contain no-repeat;
    }
  `,
  host: {
    'aria-hidden': 'true',
    '[style.--icon-url]': 'url()',
  },
})
export class Icon {
  name = input.required<string>();

  url = computed(() => `url(icons/${this.name()}.svg)`);
}