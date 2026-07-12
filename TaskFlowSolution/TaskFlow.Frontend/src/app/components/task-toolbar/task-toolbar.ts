import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { Category } from '../../models/category/category.model';
import { TaskQuery } from '../../models/task/task-query.model';
import { TaskSortBy } from '../../models/task/task-sort-by.enum';

@Component({
  selector: 'app-task-toolbar',
  imports: [
    CommonModule,
    FormsModule,
  ],
  templateUrl: './task-toolbar.html',
  styleUrl: './task-toolbar.css',
})
export class TaskToolbar {
  @Input({ required: true })
  query!: TaskQuery;

  @Input()
  categories: Category[] = [];

  @Output()
  queryChanged = new EventEmitter<TaskQuery>();

  @Output()
  createTask = new EventEmitter<void>();

  readonly TaskSortBy = TaskSortBy;

  readonly sortOptions = Object.values(TaskSortBy);

  readonly sortLabels: Record<TaskSortBy, string> = {
    [TaskSortBy.None]: 'No Sorting',
    [TaskSortBy.Name]: 'Name',
    [TaskSortBy.DueDate]: 'Due Date',
    [TaskSortBy.Priority]: 'Priority',
  };

  onSearchChanged(value: string): void {

    this.queryChanged.emit({

      ...this.query,

      page: 1,

      search: value || undefined,

    });

  }

  onCategoryChanged(value: string): void {

    this.queryChanged.emit({

      ...this.query,

      page: 1,

      categoryId: value || undefined,

    });

  }

  onSortChanged(value: TaskSortBy | undefined): void {

    this.queryChanged.emit({

      ...this.query,

      sortBy: value,

    });

  }

  toggleDirection(): void {

    this.queryChanged.emit({

      ...this.query,

      descending: !this.query.descending,

    });

  }

  clearFilters(): void {

    this.queryChanged.emit({

      ...this.query,

      page: 1,

      search: undefined,

      categoryId: undefined,

      sortBy: undefined,

      descending: false,

    });

  }

  previousPage(): void {

    if (this.query.page <= 1) {
      return;
    }

    this.queryChanged.emit({

      ...this.query,

      page: this.query.page - 1,

    });

  }

  nextPage(): void {

    this.queryChanged.emit({

      ...this.query,

      page: this.query.page + 1,

    });

  }
}