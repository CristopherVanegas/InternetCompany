import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './users-list.component.html'
})
export class UsersListComponent {

  users: any[] = [];

  newUser = {
    username: '',
    email: '',
    password: '',
    roleId: 2
  };

  message = '';

  constructor(private http: HttpClient) {
    this.loadUsers();
  }

  loadUsers() {
    this.http.get<any[]>(`${environment.apiUrl}/users`)
      .subscribe(data => this.users = data);
  }

  createUser() {
    this.http.post(`${environment.apiUrl}/users`, this.newUser)
      .subscribe({
        next: () => {
          this.message = 'Usuario creado correctamente';
          this.loadUsers();
          this.resetForm();
          this.closeModal();
        },
        error: err => {
          this.message = err.error?.message || 'Error al crear usuario';
        }
      });
  }

  approve(id: number) {
    this.http.post(`${environment.apiUrl}/users/${id}/approve`, {})
      .subscribe(() => this.loadUsers());
  }

  resetForm() {
    this.newUser = {
      username: '',
      email: '',
      password: '',
      roleId: 2
    };
  }

  closeModal() {
    const modal = document.getElementById('createUserModal');
    if (modal) {
      modal.classList.remove('show');
      modal.style.display = 'none';
      document.body.classList.remove('modal-open');
      const backdrop = document.querySelector('.modal-backdrop');
      if (backdrop) backdrop.remove();
    }
  }
}
