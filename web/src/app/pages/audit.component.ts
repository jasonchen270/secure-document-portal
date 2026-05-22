import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

interface AuditEvent {
  id: number;
  occurredAt: string;
  actorId: string | null;
  action: string;
  targetType: string | null;
  targetId: string | null;
  ipAddress: string | null;
  metadata: string | null;
}

@Component({
  selector: 'app-audit',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h1>Audit log</h1>
    <p class="muted">Most recent 100 security-relevant events.</p>

    <div class="card">
      <table>
        <thead>
          <tr>
            <th>When</th>
            <th>Action</th>
            <th>Actor</th>
            <th>Target</th>
            <th>IP</th>
            <th>Metadata</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let e of events()">
            <td class="muted">{{ e.occurredAt | date:'medium' }}</td>
            <td><code>{{ e.action }}</code></td>
            <td class="muted">{{ e.actorId || 'N/A' }}</td>
            <td class="muted">{{ e.targetType }}/{{ e.targetId }}</td>
            <td class="muted">{{ e.ipAddress || 'N/A' }}</td>
            <td class="muted">{{ e.metadata || 'N/A' }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  `
})
export class AuditComponent {
  private http = inject(HttpClient);
  events = signal<AuditEvent[]>([]);

  constructor() {
    this.http.get<AuditEvent[]>(`${environment.apiUrl}/audit?take=100`)
      .subscribe((evts) => this.events.set(evts));
  }
}
