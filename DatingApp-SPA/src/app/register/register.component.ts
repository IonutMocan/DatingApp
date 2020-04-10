import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_service/auth.service';
import { AlertifyService } from '../_service/alertify.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
    @Output() cancleRegister = new EventEmitter();
  model: any = {};

  constructor(private authService: AuthService, private aleritfy: AlertifyService) { }

  ngOnInit() {
  }

  register() {
    this.authService.register(this.model).subscribe(() => {
      this.aleritfy.success('Registration successful');
    }, error => {
      this.aleritfy.error(error);
    } );
  }

  cancel() {
    this.cancleRegister.emit(false);
   }

}
