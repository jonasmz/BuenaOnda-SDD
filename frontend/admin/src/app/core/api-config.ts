import { InjectionToken } from '@angular/core';

/**
 * URL base de la API administrativa del catálogo. En desarrollo se resuelve mediante el proxy
 * de `ng serve` (proxy.conf.json) hacia la API de backend.
 */
export const CATALOG_API_URL = new InjectionToken<string>('CATALOG_API_URL', {
  providedIn: 'root',
  factory: () => '/api/admin/catalog',
});
