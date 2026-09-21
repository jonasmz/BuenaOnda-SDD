import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CATALOG_API_URL } from '../../core/api-config';
import { Category, CategoryInput } from '../models/category';

/** Cliente HTTP de la API administrativa del catálogo (contracts/catalog-admin-api.md). */
@Injectable({ providedIn: 'root' })
export class CatalogApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(CATALOG_API_URL);

  listCategories(includeInactive = false): Observable<Category[]> {
    const params = new HttpParams().set('includeInactive', includeInactive);
    return this.http.get<Category[]>(`${this.baseUrl}/categories`, { params });
  }

  getCategory(id: string): Observable<Category> {
    return this.http.get<Category>(`${this.baseUrl}/categories/${id}`);
  }

  createCategory(input: CategoryInput): Observable<Category> {
    return this.http.post<Category>(`${this.baseUrl}/categories`, input);
  }

  updateCategory(id: string, input: CategoryInput): Observable<Category> {
    return this.http.put<Category>(`${this.baseUrl}/categories/${id}`, input);
  }
}
