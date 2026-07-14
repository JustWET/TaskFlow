import {
  Component,
  EventEmitter,
  Input,
  Output,
  SimpleChanges,
  inject,
  OnChanges,
} from '@angular/core';

import { CommonModule } from '@angular/common';
import {
  FormsModule,
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ChangeDetectorRef} from '@angular/core';

import { Task } from '../../models/task/task.model';
import { Priority } from '../../models/task/priority.enum';
import { Category } from '../../models/category/category.model';
import { SaveTaskRequest } from '../../models/task/save-task-request.model';

@Component({
  selector: 'app-task-edit-modal',
  imports: [
    FormsModule,
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: './task-edit-modal.html',
  styleUrl: './task-edit-modal.css',
})
export class TaskEditModal implements OnChanges {
  @Input()
  visible = false;

  @Input()
  task: Task | null = null;

  @Input()
  categories: Category[] = [];

  @Output()
  close = new EventEmitter<void>();

  @Output()
  save = new EventEmitter<SaveTaskRequest>();

  @Output()
  createCategory = new EventEmitter<string>();

  protected readonly Priority = Priority;
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);
  readonly priorities = Object.values(Priority);

  showCreateCategory = false;

  readonly newCategoryControl =
    this.fb.nonNullable.control('');

  readonly form = this.fb.nonNullable.group({
    name: [
      '',
      [
        Validators.required,
      ],
    ],

    description: [''],

    priority: [
      Priority.Low,
    ],

    dueDate: [''],

    categoryId: [''],
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible'] && this.visible) {
      if (this.task) {

        this.form.reset({
          name: this.task.name,
          description: this.task.description ?? '',
          priority: this.task.priority,
          dueDate: this.task.dueDate ?? '',
          categoryId: this.task.categoryId ?? '',
        });
      } else {
        this.form.reset({
          name: '',
          description: '',
          priority: Priority.Low,
          dueDate: '',
          categoryId: '',
        });
      }
    }
  }

  onClose(): void {
    this.close.emit();
  }

  onSave(): void {

    if (this.form.invalid) {
      return;
    }

    const request: SaveTaskRequest = {
      name: this.form.controls.name.value,
      description: this.form.controls.description.value || null,
      priority: this.form.controls.priority.value,
      dueDate: this.form.controls.dueDate.value || null,
      categoryId: this.form.controls.categoryId.value || null,
    };

    this.save.emit(request);
    this.cdr.detectChanges();
  }

  get isEditMode(): boolean {
    return this.task !== null;
  }

  get title(): string {
    return this.isEditMode
      ? 'Edit Task'
      : 'Create Task';
  }
  
  openCreateCategory(): void {

    this.showCreateCategory = true;

    this.newCategoryControl.reset();

  }

  cancelCreateCategory(): void {

    this.showCreateCategory = false;

    this.newCategoryControl.reset();

  }

  confirmCreateCategory(): void {

    console.log("confirmCreateCategory");
    const name =
      this.newCategoryControl.value.trim();

    if (!name) {
      console.log("failed");
      return;
    }

    this.createCategory.emit(name);

    this.showCreateCategory = false;

    this.newCategoryControl.reset();

  }
}