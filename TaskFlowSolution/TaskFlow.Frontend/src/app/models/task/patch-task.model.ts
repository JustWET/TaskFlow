import { Priority } from './priority.enum';

export interface PatchTask {
  isCompleted?: boolean;
  name?: string;
  priority?: Priority;
  dueDate?: string | null;
}