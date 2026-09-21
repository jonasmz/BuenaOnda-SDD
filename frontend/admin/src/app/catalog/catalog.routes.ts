import { Routes } from '@angular/router';

/** Rutas del módulo de catálogo; las páginas se agregan con cada historia de usuario. */
export const CATALOG_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'categories' },
  {
    path: 'categories',
    children: [
      { path: '', loadComponent: () => import('./pages/categories/category-list').then((m) => m.CategoryList) },
      { path: 'new', loadComponent: () => import('./pages/categories/category-form').then((m) => m.CategoryForm) },
      { path: ':id/edit', loadComponent: () => import('./pages/categories/category-form').then((m) => m.CategoryForm) },
    ],
  },
  {
    path: 'products',
    children: [
      { path: '', loadComponent: () => import('./pages/products/product-list').then((m) => m.ProductList) },
      { path: 'new', loadComponent: () => import('./pages/products/product-form').then((m) => m.ProductForm) },
      { path: ':id/edit', loadComponent: () => import('./pages/products/product-form').then((m) => m.ProductForm) },
    ],
  },
];
