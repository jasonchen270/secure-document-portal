import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface DocumentListItem {
  id: string;
  title: string;
  classification: string;
  ownerId: string;
  updatedAt: string;
  latestVersion: number | null;
  versionCount: number;
}

@Injectable({ providedIn: 'root' })
export class DocumentsService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/documents`;

  list(): Observable<DocumentListItem[]> {
    return this.http.get<DocumentListItem[]>(this.base);
  }

  create(title: string, classification: string) {
    return this.http.post<DocumentListItem>(this.base, { title, classification });
  }

  uploadVersion(documentId: string, file: File) {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ id: string; version: number; sha256: string; sizeBytes: number }>(
      `${this.base}/${documentId}/versions`,
      form
    );
  }

  download(documentId: string): Observable<Blob> {
    return this.http.get(`${this.base}/${documentId}/download`, { responseType: 'blob' });
  }

  delete(documentId: string) {
    return this.http.delete<void>(`${this.base}/${documentId}`);
  }
}
