import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule],   // 👈 AQUÍ
  template: `
    <h2>Usuarios</h2>
    <div *ngFor="let user of users">
      {{user.username}} - {{user.role}}
    </div>
  `
})
export class UsersListComponent {

  users: any[] = [];

  constructor(private http: HttpClient) {
    this.http.get<any[]>(`${environment.apiUrl}/users`)
      .subscribe(data => this.users = data);
  }
}
