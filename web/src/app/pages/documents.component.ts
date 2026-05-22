import { Component, ElementRef, ViewChild, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../core/auth.service';
import { DocumentListItem, DocumentsService } from '../core/documents.service';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="flex between">
      <h1>Documents</h1>
      <button class="btn btn-primary" (click)="showCreate.set(!showCreate())" *ngIf="auth.canUpload()">
        {{ showCreate() ? 'Cancel' : 'New document' }}
      </button>
    </div>

    <div class="card" *ngIf="showCreate()">
      <h3>Create document</h3>
      <div class="row">
        <div class="field">
          <label>Title</label>
          <input [(ngModel)]="newTitle" placeholder="Q4 Compliance Report"/>
        </div>
        <div class="field">
          <label>Classification</label>
          <select [(ngModel)]="newClassification">
            <option>Public</option>
            <option>Internal</option>
            <option>Confidential</option>
            <option>Restricted</option>
          </select>
        </div>
      </div>
      <div class="right">
        <button class="btn btn-primary" (click)="create()" [disabled]="!newTitle.trim()">Create</button>
      </div>
    </div>

    <div class="alert alert-error" *ngIf="error()">{{ error() }}</div>

    <div class="card">
      <table>
        <thead>
          <tr>
            <th>Title</th>
            <th>Classification</th>
            <th>Versions</th>
            <th>Updated</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let d of documents()">
            <td>{{ d.title }}</td>
            <td><span class="tag" [ngClass]="'tag-' + d.classification.toLowerCase()">{{ d.classification }}</span></td>
            <td>{{ d.versionCount }} <span class="muted" *ngIf="d.latestVersion">(v{{ d.latestVersion }})</span></td>
            <td class="muted">{{ d.updatedAt | date:'short' }}</td>
            <td class="right">
              <button class="btn" (click)="picker.click(); selected.set(d.id)" *ngIf="auth.canUpload()">Upload version</button>
              <button class="btn" (click)="download(d)" [disabled]="!d.latestVersion">Download</button>
              <button class="btn btn-danger" (click)="remove(d)">Delete</button>
            </td>
          </tr>
          <tr *ngIf="!documents().length">
            <td colspan="5" class="muted" style="text-align:center; padding: 32px">No documents yet.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <input #picker type="file" hidden (change)="upload($event)"/>
  `
})
export class DocumentsComponent {
  auth = inject(AuthService);
  private docs = inject(DocumentsService);

  documents = signal<DocumentListItem[]>([]);
  error = signal<string | null>(null);
  showCreate = signal(false);
  selected = signal<string | null>(null);

  newTitle = '';
  newClassification = 'Internal';

  @ViewChild('picker') picker!: ElementRef<HTMLInputElement>;

  constructor() {
    this.refresh();
  }

  refresh() {
    this.docs.list().subscribe({
      next: (d) => this.documents.set(d),
      error: () => this.error.set('Failed to load documents.')
    });
  }

  create() {
    this.docs.create(this.newTitle.trim(), this.newClassification).subscribe({
      next: () => {
        this.newTitle = '';
        this.showCreate.set(false);
        this.refresh();
      },
      error: () => this.error.set('Failed to create document.')
    });
  }

  upload(e: Event) {
    const input = e.target as HTMLInputElement;
    const file = input.files?.[0];
    const id = this.selected();
    if (!file || !id) return;
    this.docs.uploadVersion(id, file).subscribe({
      next: () => { input.value = ''; this.refresh(); },
      error: () => this.error.set('Upload failed.')
    });
  }

  download(d: DocumentListItem) {
    this.docs.download(d.id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = d.title;
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => this.error.set('Download failed.')
    });
  }

  remove(d: DocumentListItem) {
    this.docs.delete(d.id).subscribe({
      next: () => this.refresh(),
      error: () => this.error.set('Delete failed (you may not own this document).')
    });
  }
}
