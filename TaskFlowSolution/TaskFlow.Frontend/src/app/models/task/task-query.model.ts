import { TaskSortBy } from './task-sort-by.enum';

export interface TaskQuery {
  page: number;
  pageSize: number;
  search?: string;
  sortBy?: TaskSortBy;
  descending: boolean;
  categoryId?: string;
}