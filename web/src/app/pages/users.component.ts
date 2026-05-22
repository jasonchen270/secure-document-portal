import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

interface UserRow {
  id: string;
  email: string;
  role: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt: string | null;
}

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="flex between">
      <h1>Users</h1>
      <button class="btn btn-primary" (click)="showCreate.set(!showCreate())">
        {{ showCreate() ? 'Cancel' : 'Add user' }}
      </button>
    </div>

    <div class="card" *ngIf="showCreate()">
      <div class="row">
        <div class="field"><label>Email</label><input [(ngModel)]="email"/></div>
        <div class="field"><label>Password</label><input type="password" [(ngModel)]="password"/></div>
        <div class="field">
          <label>Role</label>
          <select [(ngModel)]="role">
            <option>Admin</option><option>Reviewer</option><option>Uploader</option>
          </select>
        </div>
      </div>
      <div class="right"><button class="btn btn-primary" (click)="create()">Create</button></div>
      <div class="alert alert-error" *ngIf="error()">{{ error() }}</div>
    </div>

    <div class="card">
      <table>
        <thead><tr><th>Email</th><th>Role</th><th>Active</th><th>Created</th><th>Last login</th></tr></thead>
        <tbody>
          <tr *ngFor="let u of users()">
            <td>{{ u.email }}</td>
            <td><span class="role-pill">{{ u.role }}</span></td>
            <td>{{ u.isActive ? 'Yes' : 'No' }}</td>
            <td class="muted">{{ u.createdAt | date:'short' }}</td>
            <td class="muted">{{ (u.lastLoginAt | date:'short') || 'N/A' }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  `
})
export class UsersComponent {
  private http = inject(HttpClient);

  users = signal<UserRow[]>([]);
  showCreate = signal(false);
  error = signal<string | null>(null);

  email = '';
  password = '';
  role = 'Uploader';

  constructor() { this.refresh(); }

  refresh() {
    this.http.get<UserRow[]>(`${environment.apiUrl}/users`).subscribe((u) => this.users.set(u));
  }

  create() {
    this.http.post(`${environment.apiUrl}/users`, {
      email: this.email, password: this.password, role: this.role
    }).subscribe({
      next: () => { this.email = ''; this.password = ''; this.showCreate.set(false); this.refresh(); },
      error: (e) => this.error.set(e?.error?.error || 'Failed to create user.')
    });
  }
}
