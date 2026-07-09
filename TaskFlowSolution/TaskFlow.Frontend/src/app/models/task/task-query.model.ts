import { Priority } from './priority.enum';

export interface TaskQuery {
  page: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  categoryId?: string;
  priority?: Priority;
  isCompleted?: boolean;
}