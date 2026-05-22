import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="app-shell">
      <header class="topbar" *ngIf="auth.user() as u; else loginBar">
        <div class="flex" style="gap: 24px">
          <div class="brand">SECURE DOCUMENT PORTAL</div>
          <nav>
            <a routerLink="/documents" routerLinkActive="active">Documents</a>
            <a routerLink="/audit" routerLinkActive="active" *ngIf="auth.canReview()">Audit</a>
            <a routerLink="/users" routerLinkActive="active" *ngIf="auth.isAdmin()">Users</a>
          </nav>
        </div>
        <div class="user">
          <span>{{ u.email }}</span>
          <span class="role-pill">{{ u.role }}</span>
          <button class="btn" (click)="logout()">Sign out</button>
        </div>
      </header>
      <ng-template #loginBar>
        <header class="topbar">
          <div class="brand">SECURE DOCUMENT PORTAL</div>
        </header>
      </ng-template>

      <main>
        <router-outlet />
      </main>
    </div>
  `
})
export class AppComponent {
  auth = inject(AuthService);
  private router = inject(Router);

  logout() {
    this.auth.logout().subscribe({
      complete: () => this.router.navigateByUrl('/login')
    });
  }
}
