import { Priority } from './priority.enum';

export interface SaveTaskRequest  {
  name: string;
  description: string | null;
  priority: Priority;
  dueDate: string | null;
  categoryId: string | null;
}