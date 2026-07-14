import {
  Component,
  EventEmitter,
  Input,
  Output,
} from '@angular/core';

import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import { Category } from '../../models/category/category.model';

@Component({
  selector: 'app-category-manager-modal',
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
  ],
  templateUrl: './category-manager-modal.html',
  styleUrl: './category-manager-modal.css',
})
export class CategoryManagerModal {

  @Input()
  visible = false;

  @Input()
  categories: Category[] = [];

  @Output()
  close = new EventEmitter<void>();

  @Output()
  create = new EventEmitter<string>();

  @Output()
  rename = new EventEmitter<{
    id: string;
    name: string;
  }>();

  @Output()
  delete = new EventEmitter<string>();

  private readonly fb = new FormBuilder();

  readonly createForm = this.fb.nonNullable.group({
    name: [
      '',
      [
        Validators.required,
        Validators.maxLength(100),
      ],
    ],
  });

  editingCategoryId: string | null = null;

  editingName = '';

  onClose(): void {

    this.cancelEdit();

    this.createForm.reset();

    this.close.emit();

  }

  onCreate(): void {

    if (this.createForm.invalid) {
      return;
    }

    const name =
      this.createForm.controls.name.value.trim();

    if (!name) {
      return;
    }

    this.create.emit(name);

    this.createForm.reset();

  }

  startEdit(category: Category): void {

    this.editingCategoryId = category.id;

    this.editingName = category.name;

  }

  cancelEdit(): void {

    this.editingCategoryId = null;

    this.editingName = '';

  }

  saveEdit(categoryId: string): void {

    const name =
      this.editingName.trim();

    if (!name) {
      return;
    }

    this.rename.emit({
      id: categoryId,
      name,
    });

    this.cancelEdit();

  }

  onDelete(categoryId: string): void {

    this.delete.emit(categoryId);

  }

  isEditing(categoryId: string): boolean {

    return this.editingCategoryId === categoryId;

  }

}