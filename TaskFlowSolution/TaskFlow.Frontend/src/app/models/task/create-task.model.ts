import { Priority } from './priority.enum';

export interface CreateTask {
  name: string;
  description: string | null;
  priority: Priority;
  dueDate: string | null;
  categoryId: string | null;
}