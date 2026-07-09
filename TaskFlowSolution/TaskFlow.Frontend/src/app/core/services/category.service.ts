import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

import { Category } from '../../models/category/category.model';
import { CreateOrUpdateCategory } from '../../models/category/create-or-update-category.model';

@Injectable({
  providedIn: 'root',
})
export class CategoryService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/categories`;

  getAll(): Observable<Category[]> {
    return this.http.get<Category[]>(this.apiUrl);
  }

  getById(categoryId: string): Observable<Category> {
    return this.http.get<Category>(
      `${this.apiUrl}/${categoryId}`
    );
  }

  create(request: CreateOrUpdateCategory): Observable<Category> {
    return this.http.post<Category>(
      this.apiUrl,
      request
    );
  }

  update(
    categoryId: string,
    request: CreateOrUpdateCategory,
  ): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/${categoryId}`,
      request
    );
  }

  delete(categoryId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${categoryId}`
    );
  }
}