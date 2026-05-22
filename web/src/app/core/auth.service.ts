import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AuthUser {
  email: string;
  role: 'Admin' | 'Reviewer' | 'Uploader';
}

interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  accessExpiresAt: string;
  role: AuthUser['role'];
  email: string;
}

const STORAGE_KEY = 'sdp.auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);

  private state = signal<{ user: AuthUser; access: string; refresh: string } | null>(this.load());

  user = () => this.state()?.user ?? null;
  accessToken = () => this.state()?.access ?? null;

  isAdmin = () => this.user()?.role === 'Admin';
  canReview = () => {
    const r = this.user()?.role;
    return r === 'Admin' || r === 'Reviewer';
  };
  canUpload = () => {
    const r = this.user()?.role;
    return r === 'Admin' || r === 'Reviewer' || r === 'Uploader';
  };

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/auth/login`, { email, password })
      .pipe(tap((r) => this.persist(r)));
  }

  logout(): Observable<void> {
    const refresh = this.state()?.refresh;
    this.clear();
    if (!refresh) return new Observable((sub) => sub.complete());
    return this.http.post<void>(`${environment.apiUrl}/auth/logout`, { refreshToken: refresh });
  }

  refresh(): Observable<LoginResponse> {
    const refresh = this.state()?.refresh ?? '';
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/auth/refresh`, { refreshToken: refresh })
      .pipe(tap((r) => this.persist(r)));
  }

  private persist(r: LoginResponse) {
    const next = {
      user: { email: r.email, role: r.role },
      access: r.accessToken,
      refresh: r.refreshToken
    };
    this.state.set(next);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
  }

  private clear() {
    this.state.set(null);
    localStorage.removeItem(STORAGE_KEY);
  }

  private load() {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }
}
