import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

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
  @Output()
  searchChanged = new EventEmitter<string>();

  @Output()
  sortChanged = new EventEmitter<string>();

  @Output()
  filterChanged = new EventEmitter<string>();

  @Output()
  createTask = new EventEmitter<void>();

  searchText = '';

  selectedSort = 'name';

  selectedFilter = 'all';

  onSearchChange(): void {
    this.searchChanged.emit(this.searchText);
  }

  onSortChange(): void {
    this.sortChanged.emit(this.selectedSort);
  }

  onFilterChange(): void {
    this.filterChanged.emit(this.selectedFilter);
  }

  onCreateTask(): void {
    this.createTask.emit();
  }
}