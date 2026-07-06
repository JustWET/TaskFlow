import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';

import { UserAuthModel } from '../../models/auth/user-auth.model';
import { AuthResponseModel } from '../../models/auth/auth-response.model';

import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/users`;
  private readonly loginUrl = `${this.apiUrl}/login`;
  private readonly registerUrl = `${this.apiUrl}/register`;

  private readonly http = inject(HttpClient);
  private readonly tokenService = inject(TokenService);

  login(request: UserAuthModel): Observable<AuthResponseModel> {
    return this.http
      .post<AuthResponseModel>(this.loginUrl, request)
      .pipe(
        tap(response => this.tokenService.saveToken(response.token))
      );
  }

  register(request: UserAuthModel): Observable<AuthResponseModel> {
    return this.http
      .post<AuthResponseModel>(this.registerUrl, request)
      .pipe(
        tap(response => this.tokenService.saveToken(response.token))
      );
  }

  logout(): void {
    this.tokenService.removeToken();
  }
}