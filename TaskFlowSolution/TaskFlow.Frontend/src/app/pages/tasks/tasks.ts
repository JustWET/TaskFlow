import { Component, OnInit, inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { ChangeDetectorRef} from '@angular/core';

import { forkJoin } from 'rxjs';

import { Task } from '../../models/task/task.model';
import { Priority } from './../../models/task/priority.enum';
import { TaskSortBy } from './../../models/task/task-sort-by.enum';
import { Category } from '../../models/category/category.model';

import { TaskQuery } from '../../models/task/task-query.model';
import { SaveTaskRequest } from '../../models/task/save-task-request.model';

import { TaskService } from '../../core/services/task.service';
import { CategoryService } from '../../core/services/category.service';

import { TaskToolbar } from '../../components/task-toolbar/task-toolbar';
import { TaskTable } from '../../components/task-table/task-table';
import { TaskEditModal } from '../../components/task-edit-modal/task-edit-modal';

@Component({
  selector: 'app-tasks',
  imports: [
    TaskToolbar,
    TaskTable,
    TaskEditModal,
  ],
  templateUrl: './tasks.html',
  styleUrl: './tasks.css',
})
export class Tasks implements OnInit {
  private readonly cdr = inject(ChangeDetectorRef);

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  private readonly taskService = inject(TaskService);

  private readonly categoryService = inject(CategoryService);

  taskListId = '';

  activeTasks: Task[] = [];

  completedTasks: Task[] = [];

  categories: Category[] = [];

  selectedTask: Task | null = null;

  showEditModal = false;

  activeCollapsed = false;

  completedCollapsed = true;

  isLoading = false;

  errorMessage = '';

  query: TaskQuery = {
    page: 1,
    pageSize: 15,
    search: undefined,
    sortBy: undefined,
    descending: true,
    categoryId: undefined,
  };

  ngOnInit(): void {

    this.taskListId =
      this.route.snapshot.paramMap.get('id') ?? '';

    this.loadData();

  }

  private loadData(): void {

    this.isLoading = true;

    this.errorMessage = '';

    forkJoin({
      tasks: this.taskService.getAll(
        this.taskListId,
        this.query,
      ),

      categories: this.categoryService.getAll(),
    }).subscribe({

      next: ({ tasks, categories }) => {

        this.categories = categories;

        this.setTasks(tasks.items);

      },

      error: () => {

        this.errorMessage =
          'Unable to load data.';

      },

      complete: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      },

    });

  }

  private setTasks(tasks: Task[]): void {

    this.activeTasks =
      tasks.filter(t => !t.isCompleted);

    this.completedTasks =
      tasks.filter(t => t.isCompleted);

  }

  goBack(): void {

    this.router.navigate([
      '/task-lists',
    ]);

  }

  createTask(): void {

    this.selectedTask = null;

    this.showEditModal = true;

  }

  openEditTask(task: Task): void {

    this.selectedTask = structuredClone(task);

    this.showEditModal = true;

  }

  closeEditTask(): void {

    this.selectedTask = null;

    this.showEditModal = false;

  }

  saveTask(request: SaveTaskRequest): void {

    if (this.selectedTask) {

      this.taskService
        .update(
          this.selectedTask.id,
          request,
        )
        .subscribe({

          next: () => {

            const index =
              this.findTaskIndex(
                this.selectedTask!.id,
              );

            if (index == null) {
              return;
            }

            const updatedTask: Task = {
              ...this.selectedTask!,
              ...request,
            };

            this.replaceTask(updatedTask);

            this.closeEditTask();

          },

        });

      return;

    }

    const createRequest: SaveTaskRequest = {
      name: request.name,
      description: request.description,
      priority: request.priority,
      dueDate: request.dueDate,
      categoryId: request.categoryId,
    };

    this.taskService
      .create(
        this.taskListId,
        createRequest,
      )
      .subscribe({

        next: task => {

          this.activeTasks.unshift(task);

          this.closeEditTask();

        },

      });

  }

  deleteTask(task: Task): void {

    this.taskService
      .delete(task.id)
      .subscribe({

        next: () => {

          this.activeTasks =
            this.activeTasks.filter(
              t => t.id !== task.id,
            );

          this.completedTasks =
            this.completedTasks.filter(
              t => t.id !== task.id,
            );

          if (
            this.selectedTask?.id === task.id
          ) {

            this.closeEditTask();

          }

        },

      });

  }

  private replaceTask(
    updatedTask: Task,
  ): void {

    this.activeTasks =
      this.activeTasks.map(task =>
        task.id === updatedTask.id
          ? updatedTask
          : task,
      );

    this.completedTasks =
      this.completedTasks.map(task =>
        task.id === updatedTask.id
          ? updatedTask
          : task,
      );

    this.setTasks([
      ...this.activeTasks,
      ...this.completedTasks,
    ]);

  }

  private findTaskIndex(
    taskId: string,
  ): number | null {

    const activeIndex =
      this.activeTasks.findIndex(
        t => t.id === taskId,
      );

    if (activeIndex >= 0) {
      return activeIndex;
    }

    const completedIndex =
      this.completedTasks.findIndex(
        t => t.id === taskId,
      );

    if (completedIndex >= 0) {
      return completedIndex;
    }

    return null;

  }

    toggleCompleted(task: Task): void {

    const request =
      task.isCompleted
        ? this.taskService.uncomplete(task.id)
        : this.taskService.complete(task.id);

    request.subscribe({

      next: () => {

        task.isCompleted = !task.isCompleted;

        this.setTasks([
          ...this.activeTasks,
          ...this.completedTasks,
        ]);

      },

    });

  }

  renameTask(task: Task): void {

    this.taskService
      .rename(task.id, {
        name: task.name,
      })
      .subscribe();

  }

  changePriority(task: Task): void {

    this.taskService
      .changePriority(task.id, {
        priority: task.priority,
      })
      .subscribe();

  }

  changeDueDate(task: Task): void {

    this.taskService
      .changeDueDate(task.id, {
        dueDate: task.dueDate,
      })
      .subscribe();

  }

  onQueryChanged(query: TaskQuery): void {

    this.query = query;

    this.reloadTasks();

  }

  clearFilters(): void {

    this.query.search = undefined;

    this.query.categoryId = undefined;

    this.query.sortBy = TaskSortBy.None;

    this.query.descending = true;

    this.reloadTasks();

  }

  private reloadTasks(): void {

    this.isLoading = true;

    this.taskService
      .getAll(
        this.taskListId,
        this.query,
      )
      .subscribe({

        next: result => {

          this.setTasks(result.items);

        },

        error: () => {

          this.errorMessage =
            'Unable to load tasks.';

        },

        complete: () => {

          this.isLoading = false;

        },

      });

  }
}