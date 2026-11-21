import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';

// A aplicação registra o locale pt-BR em app.config.ts; os testes precisam fazer o mesmo
// para que o DatePipe funcione nos templates.
registerLocaleData(localePt);
