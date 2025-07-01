import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styles: [
  ]
})
export class NavComponent implements OnInit {

  model: any = {}

  constructor() { }

  ngOnInit(): void {
  }

  login() {
    console.log(this.model);
  }

}
