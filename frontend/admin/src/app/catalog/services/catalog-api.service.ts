import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CATALOG_API_URL } from '../../core/api-config';
import { Category, CategoryInput } from '../models/category';
import { CreateProductInput, OptionInput, Product, UpdateProductInput } from '../models/product';

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

  listProducts(categoryId?: string, includeInactive = false): Observable<Product[]> {
    let params = new HttpParams().set('includeInactive', includeInactive);
    if (categoryId) {
      params = params.set('categoryId', categoryId);
    }
    return this.http.get<Product[]>(`${this.baseUrl}/products`, { params });
  }

  getProduct(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/products/${id}`);
  }

  createProduct(input: CreateProductInput): Observable<Product> {
    return this.http.post<Product>(`${this.baseUrl}/products`, input);
  }

  updateProduct(id: string, input: UpdateProductInput): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}/products/${id}`, input);
  }

  deactivateCategory(id: string): Observable<Category> {
    return this.http.post<Category>(`${this.baseUrl}/categories/${id}/deactivate`, {});
  }

  reactivateCategory(id: string): Observable<Category> {
    return this.http.post<Category>(`${this.baseUrl}/categories/${id}/reactivate`, {});
  }

  deactivateProduct(id: string): Observable<Product> {
    return this.http.post<Product>(`${this.baseUrl}/products/${id}/deactivate`, {});
  }

  reactivateProduct(id: string): Observable<Product> {
    return this.http.post<Product>(`${this.baseUrl}/products/${id}/reactivate`, {});
  }

  updateOption(productId: string, optionId: string, input: Omit<OptionInput, 'isMarkedAvailable'>): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}/products/${productId}/options/${optionId}`, input);
  }

  setOptionAvailability(productId: string, optionId: string, isMarkedAvailable: boolean): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}/products/${productId}/options/${optionId}/availability`, {
      isMarkedAvailable,
    });
  }

  deactivateOption(productId: string, optionId: string): Observable<Product> {
    return this.http.post<Product>(`${this.baseUrl}/products/${productId}/options/${optionId}/deactivate`, {});
  }

  reactivateOption(productId: string, optionId: string): Observable<Product> {
    return this.http.post<Product>(`${this.baseUrl}/products/${productId}/options/${optionId}/reactivate`, {});
  }

  deleteOption(productId: string, optionId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/products/${productId}/options/${optionId}`);
  }
}
