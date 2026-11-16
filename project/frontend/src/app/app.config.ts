import { ApplicationConfig, LOCALE_ID, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';
import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';
import { PlaceProvider } from './core/places/place-provider';
import { NominatimPlaceProvider } from './core/places/nominatim-place.provider';
import { PhotonPlaceProvider } from './core/places/photon-place.provider';
import { FallbackPlaceProvider } from './core/places/fallback-place.provider';

registerLocaleData(localePt);

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    { provide: LOCALE_ID, useValue: 'pt-BR' },
    PhotonPlaceProvider,
    NominatimPlaceProvider,
    { provide: PlaceProvider, useClass: FallbackPlaceProvider },
  ]
};