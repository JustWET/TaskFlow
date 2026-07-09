import { Priority } from './priority.enum';
import { Category } from '../category/category.model';

export interface Task {
  id: string;
  taskListId: string;
  categoryId: string | null;
  name: string;
  isCompleted: boolean;
  dueDate: string | null;
  priority: Priority;
  description: string | null;
  category: Category | null;
}