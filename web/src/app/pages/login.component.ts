import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="center">
      <div class="card login-card">
        <h2>Sign in</h2>
        <p class="muted" style="margin-top:-8px">Use one of the seeded accounts to explore.</p>

        <div class="alert alert-error" *ngIf="error()">{{ error() }}</div>

        <form (submit)="submit($event)">
          <div class="field">
            <label>Email</label>
            <input type="email" [(ngModel)]="email" name="email" required autocomplete="username"/>
          </div>
          <div class="field">
            <label>Password</label>
            <input type="password" [(ngModel)]="password" name="password" required autocomplete="current-password"/>
          </div>
          <button class="btn btn-primary" type="submit" [disabled]="loading()" style="width:100%">
            {{ loading() ? 'Signing in...' : 'Sign in' }}
          </button>
        </form>

        <div class="muted" style="margin-top: 16px; font-size: 12px">
          <strong>Seeded demo users</strong> (password: <code>ChangeMe!123</code>)
          <ul style="padding-left: 18px; margin: 6px 0 0">
            <li>admin&#64;portal.local (Admin)</li>
            <li>reviewer&#64;portal.local (Reviewer)</li>
            <li>uploader&#64;portal.local (Uploader)</li>
          </ul>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  email = 'admin@portal.local';
  password = 'ChangeMe!123';
  loading = signal(false);
  error = signal<string | null>(null);

  submit(e: Event) {
    e.preventDefault();
    this.loading.set(true);
    this.error.set(null);
    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigateByUrl('/documents');
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.error === 'invalid_credentials'
          ? 'Invalid email or password.'
          : 'Could not sign in. Is the API running?');
      }
    });
  }
}
