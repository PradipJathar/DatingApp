import { Component, OnInit } from '@angular/core';
import { User } from 'src/app/_models/user';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  
  users: Partial<User[]>;

  constructor(private adminService: AdminService) { }

  ngOnInit(): void {
    this.getUserWithRoles();
  }

  getUserWithRoles() {
    return this.adminService.getUserWithRoles().subscribe(users => {
      this.users = users;
    })
  }

}
