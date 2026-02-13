import { Routes } from '@angular/router';
import { LoginComponent } from './auth/login/login.component';
import { UsersListComponent } from './users/users-list/users-list.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'users', component: UsersListComponent, canActivate: [authGuard] },
  { path: '', redirectTo: 'login', pathMatch: 'full' }
];
