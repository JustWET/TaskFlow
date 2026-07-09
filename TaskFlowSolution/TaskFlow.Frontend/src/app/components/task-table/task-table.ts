import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

import { Task } from '../../models/task/task.model';
import { TaskRow } from '../task-row/task-row';

@Component({
  selector: 'app-task-table',
  imports: [
    CommonModule,
    TaskRow,
  ],
  templateUrl: './task-table.html',
  styleUrl: './task-table.css',
})
export class TaskTable {
  @Input({ required: true })
  tasks: Task[] = [];

  @Input()
  title = '';

  @Input()
  collapsed = false;

  @Output()
  collapsedChange = new EventEmitter<boolean>();

  @Output()
  edit = new EventEmitter<Task>();

  @Output()
  delete = new EventEmitter<Task>();

  @Output()
  rename = new EventEmitter<Task>();

  @Output()
  completedChanged = new EventEmitter<Task>();

  @Output()
  priorityChanged = new EventEmitter<Task>();

  @Output()
  dueDateChanged = new EventEmitter<Task>();

  toggle(): void {
    this.collapsed = !this.collapsed;
    this.collapsedChange.emit(this.collapsed);
  }
}