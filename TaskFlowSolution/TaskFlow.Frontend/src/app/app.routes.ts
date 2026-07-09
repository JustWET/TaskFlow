import { Routes } from '@angular/router';

import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { TaskLists } from './pages/task-lists/task-lists';
import { Tasks } from './pages/tasks/tasks';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    component: Login,
  },
  {
    path: 'register',
    component: Register,
  },
  {
    path: 'task-lists',
    component: TaskLists,
  },
  {
    path: 'task-lists/:id',
    component: Tasks,
  },
];