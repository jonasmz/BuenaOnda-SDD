import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'catalog' },
  {
    path: 'catalog',
    loadChildren: () => import('./catalog/catalog.routes').then((m) => m.CATALOG_ROUTES),
  },
];
