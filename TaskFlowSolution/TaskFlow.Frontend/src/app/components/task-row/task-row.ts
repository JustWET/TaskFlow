import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { Task } from '../../models/task/task.model';
import { Priority } from '../../models/task/priority.enum';

@Component({
  selector: 'app-task-row',
  imports: [
    CommonModule,
    FormsModule,
  ],
  templateUrl: './task-row.html',
  styleUrl: './task-row.css',
})
export class TaskRow {
  @Input({ required: true })
  task!: Task;

  readonly Priority = Priority;

  @Output()
  completedChanged = new EventEmitter<Task>();

  @Output()
  rename = new EventEmitter<Task>();

  @Output()
  priorityChanged = new EventEmitter<Task>();

  @Output()
  dueDateChanged = new EventEmitter<Task>();

  @Output()
  edit = new EventEmitter<Task>();

  @Output()
  delete = new EventEmitter<Task>();

  onCompletedChanged(): void {
    this.completedChanged.emit(this.task);
  }

  onRename(): void {
    this.rename.emit(this.task);
  }

  onPriorityChanged(): void {
    this.priorityChanged.emit(this.task);
  }

  onDueDateChanged(): void {
    this.dueDateChanged.emit(this.task);
    console.log(this.task.dueDate);
  }

  onEdit(): void {
    this.edit.emit(this.task);
  }

  onDelete(): void {
    this.delete.emit(this.task);
  }
}