import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './users-list.component.html',
  styleUrls: ['./users-list.component.scss']
})
export class UsersListComponent {

  users: any[] = [];

  constructor(private http: HttpClient) {
    this.http.get<any[]>(`${environment.apiUrl}/users`)
      .subscribe(data => {
        console.log(data);
        this.users = data;
      });
  }
}
