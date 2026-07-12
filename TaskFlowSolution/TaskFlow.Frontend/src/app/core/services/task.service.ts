import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

import { Task } from '../../models/task/task.model';
import { SaveTaskRequest } from '../../models/task/save-task-request.model';

import { RenameTask } from '../../models/task/rename-task.model';
import { ChangePriority } from '../../models/task/change-priority.model';
import { ChangeDueDate } from '../../models/task/change-due-date.model';

import { PagedResult } from '../../models/common/paged-result.model';
import { TaskQuery } from '../../models/task/task-query.model';

@Injectable({
  providedIn: 'root',
})
export class TaskService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/tasks`;

  getById(taskId: string): Observable<Task> {
    return this.http.get<Task>(
      `${this.apiUrl}/${taskId}`
    );
  }

  getAll(
    taskListId: string,
    query: TaskQuery
  ): Observable<PagedResult<Task>> {

    return this.http.get<PagedResult<Task>>(
      this.apiUrl,
      {
        params: {
          taskListId,
          page: query.page,
          pageSize: query.pageSize,
          search: query.search ?? '',
          sortBy: query.sortBy ?? '',
          descending: query.descending ?? '',
          categoryId: query.categoryId ?? '',
        },
      }
    )
    .pipe(
      map(result => ({
        ...result,
        items: result.items.map(task => this.normalizeTask(task)),
      }))
    );
  }

  create(
    taskListId: string,
    request: SaveTaskRequest
  ): Observable<Task> {

    return this.http.post<Task>(
      `${this.apiUrl}?taskListId=${taskListId}`,
      request
    );
  }

  update(
    taskId: string,
    request: SaveTaskRequest
  ): Observable<void> {

    return this.http.put<void>(
      `${this.apiUrl}/${taskId}`,
      request
    );
  }

  delete(taskId: string): Observable<void> {

    return this.http.delete<void>(
      `${this.apiUrl}/${taskId}`
    );
  }

  complete(taskId: string): Observable<void> {

    return this.http.patch<void>(
      `${this.apiUrl}/${taskId}/complete`,
      {}
    );
  }

  uncomplete(taskId: string): Observable<void> {

    return this.http.patch<void>(
      `${this.apiUrl}/${taskId}/uncomplete`,
      {}
    );
  }

  rename(
    taskId: string,
    request: RenameTask
  ): Observable<void> {

    return this.http.patch<void>(
      `${this.apiUrl}/${taskId}/name`,
      request
    );
  }

  changePriority(
    taskId: string,
    request: ChangePriority
  ): Observable<void> {

    return this.http.patch<void>(
      `${this.apiUrl}/${taskId}/priority`,
      request
    );
  }

  changeDueDate(
    taskId: string,
    request: ChangeDueDate
  ): Observable<void> {

    return this.http.patch<void>(
      `${this.apiUrl}/${taskId}/due-date`,
      request
    );
  }

  private normalizeTask(task: Task): Task {

    return {
      ...task,

      dueDate: task.dueDate
        ? task.dueDate.substring(0, 10)
        : null,
    };

  }
}