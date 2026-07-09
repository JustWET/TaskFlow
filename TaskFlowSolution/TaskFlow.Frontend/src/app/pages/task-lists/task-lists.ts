import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';

import { TaskList } from '../../models/task-list/task-list.model';
import { TaskListService } from '../../core/services/task-list.service';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HostListener } from '@angular/core';
import { ChangeDetectorRef} from '@angular/core';

@Component({
  selector: 'app-task-lists',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    //RouterLink,
  ],
  templateUrl: './task-lists.html',
  styleUrl: './task-lists.css',
})
export class TaskLists {
  private readonly taskListService = inject(TaskListService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  taskLists: TaskList[] = [];

  isLoading = true;

  errorMessage = '';

  private readonly fb = inject(FormBuilder);

  showCreateModal = false;

  showEditModal = false;

  showDeleteModal = false;

  isUpdating = false;

  isDeleting = false;

  editingTaskList: TaskList | null = null;

  deletingTaskList: TaskList | null = null;

  readonly editForm = this.fb.nonNullable.group({
    name: [
      '',
      [
        Validators.required,
      ],
    ],
  });

  isCreating = false;

  readonly createForm = this.fb.nonNullable.group({
    name: [
      '',
      [
        Validators.required,
      ],
    ],
  });

  openCreateModal(): void {
    this.createForm.reset();
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
    this.createForm.reset();
    this.isCreating = false;
  }

  createTaskList(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.isCreating = true;

    this.taskListService
      .create(this.createForm.getRawValue())
      .subscribe({
        next: taskList => {
          this.taskLists.unshift(taskList);

          this.closeCreateModal();
        },

        complete: () => {
          this.isCreating = false;
        },
      });
  }

  ngOnInit(): void {
    this.loadTaskLists();
  }

  loadTaskLists(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.taskListService.getAll().subscribe({
      next: taskLists => {
        this.taskLists = taskLists;
      },

      error: () => {
        this.errorMessage = 'Unable to load task lists.';
      },

      complete: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  openTaskList(taskList: TaskList): void {
    this.router.navigate(['/task-lists', taskList.id]);
  }

  openEditModal(taskList: TaskList): void {
    this.editingTaskList = taskList;

    this.editForm.setValue({
      name: taskList.name,
    });

    this.showEditModal = true;
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.editingTaskList = null;
    this.editForm.reset();
  }

  updateTaskList(): void {
    if (
      this.editForm.invalid ||
      !this.editingTaskList
    ) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.isUpdating = true;

    this.taskListService.update(
      this.editingTaskList.id,
      this.editForm.getRawValue()
    )
    .subscribe({
      next: () => {
        this.editingTaskList!.name =
          this.editForm.getRawValue().name;

        this.closeEditModal();
      },

      complete: () => {
        this.isUpdating = false;
      }
    });
  }

  openDeleteModal(taskList: TaskList): void {
    this.deletingTaskList = taskList;
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.deletingTaskList = null;
  }

  deleteTaskList(): void {
    if (!this.deletingTaskList)
      return;

    this.isDeleting = true;

    this.taskListService
      .delete(this.deletingTaskList.id)
      .subscribe({
        next: () => {
          this.taskLists =
            this.taskLists.filter(
              t => t.id !== this.deletingTaskList!.id
            );

          this.closeDeleteModal();
        },

        complete: () => {
          this.isDeleting = false;
          this.cdr.detectChanges();
        }
      });
  }

  @HostListener('document:keydown.escape')
  onEscapePressed(): void {
    if (this.showCreateModal) {
      this.closeCreateModal();
    }
  }
}