import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';

interface TodoItem { id: string; listId: string; description: string; dueAt: string | null; importance: number; completedAt: string | null; }
interface TodoList { id: string; title: string; description: string; version: number; items: TodoItem[]; }

@Component({
  imports: [FormsModule], selector: 'app-root', styleUrl: './app.scss', templateUrl: './app.html',
})
export class App implements OnInit {
  private readonly http = inject(HttpClient);
  protected readonly lists = signal<TodoList[]>([]);
  protected readonly selectedId = signal<string | null>(null);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal('');
  protected readonly newTitle = signal('');
  protected readonly newListDescription = signal('');
  protected readonly newItemDescription = signal('');
  protected readonly newDueAt = signal('');
  protected readonly newImportance = signal(1);
  protected readonly filter = signal<'all' | 'open' | 'done'>('all');

  ngOnInit(): void { this.loadLists(); }
  protected get selected(): TodoList | undefined { return this.lists().find(list => list.id === this.selectedId()); }
  protected get visibleItems(): TodoItem[] {
    const items = this.selected?.items ?? [];
    return items.filter(item => this.filter() === 'all' || (this.filter() === 'done') === !!item.completedAt)
      .sort((a, b) => Number(!!a.completedAt) - Number(!!b.completedAt) || b.importance - a.importance || a.description.localeCompare(b.description));
  }
  protected get openCount(): number { return this.selected?.items.filter(item => !item.completedAt).length ?? 0; }
  protected get doneCount(): number { return (this.selected?.items.length ?? 0) - this.openCount; }

  protected loadLists(selectId?: string): void {
    this.loading.set(true); this.error.set('');
    this.http.get<TodoList[]>('/api/lists').subscribe({
      next: lists => { this.lists.set(lists); this.selectedId.set(selectId && lists.some(list => list.id === selectId) ? selectId : (this.selectedId() && lists.some(list => list.id === this.selectedId()) ? this.selectedId() : lists[0]?.id ?? null)); this.loading.set(false); },
      error: error => { this.showError(error); this.loading.set(false); },
    });
  }
  protected createList(): void {
    const title = this.newTitle().trim(); if (!title) return;
    this.run(this.http.post<TodoList>('/api/lists', { title, description: this.newListDescription().trim() }), list => { this.newTitle.set(''); this.newListDescription.set(''); this.loadLists(list.id); });
  }
  protected addItem(): void {
    const list = this.selected; const description = this.newItemDescription().trim(); if (!list || !description) return;
    const dueAt = this.newDueAt() ? new Date(`${this.newDueAt()}T23:59:00`).toISOString() : null;
    this.run(this.http.post<TodoList>(`/api/lists/${list.id}/items`, { description, dueAt, importance: Number(this.newImportance()) }), updated => { this.newItemDescription.set(''); this.newDueAt.set(''); this.updateList(updated); });
  }
  protected toggle(item: TodoItem): void { this.change(item, 'completion', { completed: !item.completedAt }); }
  protected saveDescription(item: TodoItem, value: string): void { const description = value.trim(); if (description && description !== item.description) this.change(item, 'description', { description }); }
  protected saveImportance(item: TodoItem, value: string): void { const importance = Number(value); if (importance !== item.importance) this.change(item, 'importance', { importance }); }
  protected saveDueDate(item: TodoItem, value: string): void { const dueAt = value ? new Date(`${value}T23:59:00`).toISOString() : null; if ((item.dueAt ? item.dueAt.slice(0, 10) : '') !== value) this.change(item, 'due-date', { dueAt }); }
  protected dateValue(value: string | null): string { return value ? value.slice(0, 10) : ''; }
  protected formattedDate(value: string | null): string { return value ? new Intl.DateTimeFormat('it-IT', { day: 'numeric', month: 'short', year: 'numeric' }).format(new Date(value)) : ''; }
  protected isOverdue(value: string | null, completedAt: string | null): boolean { return !!value && !completedAt && new Date(value).getTime() < new Date().setHours(0, 0, 0, 0); }
  private change(item: TodoItem, route: string, body: object): void { this.run(this.http.put<TodoList>(`/api/lists/${item.listId}/items/${item.id}/${route}`, body), updated => this.updateList(updated)); }
  private updateList(updated: TodoList): void { this.lists.update(lists => lists.map(list => list.id === updated.id ? updated : list)); }
  private run<T>(request: Observable<T>, done: (value: T) => void): void {
    this.saving.set(true); this.error.set('');
    request.subscribe({ next: value => { done(value); this.saving.set(false); }, error: error => { this.showError(error); this.saving.set(false); } });
  }
  private showError(error: unknown): void {
    const response = error as HttpErrorResponse;
    const validation = response.error?.errors ? Object.values(response.error.errors as Record<string, string[]>).flat()[0] : null;
    this.error.set(validation ?? response.error?.error ?? (response.status === 0 ? 'Impossibile raggiungere il server. Controlla che lo stack sia avviato.' : `Operazione non riuscita (${response.status}).`));
  }
}
