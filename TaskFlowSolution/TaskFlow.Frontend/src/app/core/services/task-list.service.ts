import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

import { TaskList } from '../../models/task-list/task-list.model';
import { CreateTaskListRequest } from '../../models/task-list/create-task-list.model';
import { UpdateTaskListRequest } from '../../models/task-list/update-task-list.model';

@Injectable({
  providedIn: 'root',
})
export class TaskListService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/tasklists`;

  getAll(): Observable<TaskList[]> {
    return this.http.get<TaskList[]>(this.apiUrl);
  }

  getById(taskListId: string): Observable<TaskList> {
    return this.http.get<TaskList>(
      `${this.apiUrl}/${taskListId}`
    );
  }

  create(request: CreateTaskListRequest): Observable<TaskList> {
    return this.http.post<TaskList>(
      this.apiUrl,
      request
    );
  }

  update(
    taskListId: string,
    request: UpdateTaskListRequest
  ): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/${taskListId}`,
      request
    );
  }

  delete(taskListId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${taskListId}`
    );
  }
}