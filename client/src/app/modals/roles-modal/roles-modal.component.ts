import { Component, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.css']
})
export class RolesModalComponent implements OnInit {

  title: string;
  list: any[];
  closeBtnName: string;
  bsModalRef: BsModalRef;

  constructor(bsModalRef: BsModalRef) {
    this.bsModalRef = bsModalRef;
   }

  ngOnInit(): void {
  }

}
