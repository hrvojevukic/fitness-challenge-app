import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-register-user',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register-user.component.html',
  styleUrl: './register-user.component.css',
})
export class RegisterUserComponent {
  private userService = inject(UserService);
  private auth = inject(AuthService);
  private router = inject(Router);

  firstName = '';
  lastName = '';

  submitting = signal(false);
  error = signal<string | null>(null);

  submit(): void {
    const firstName = this.firstName.trim();
    const lastName = this.lastName.trim();

    if (!firstName || !lastName) {
      this.error.set('First and last name are both required.');
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    this.userService.register(firstName, lastName).subscribe({
      next: () => {
        this.auth.login(firstName, lastName).subscribe({
          next: () => this.router.navigateByUrl('/log-activity'),
          error: () => {
            this.submitting.set(false);
            this.error.set('Registered, but automatic login failed. Try logging in manually.');
          },
        });
      },
      error: (err: HttpErrorResponse) => {
        this.submitting.set(false);
        if (err.status === 409) {
          this.error.set(`A user named '${firstName} ${lastName}' is already registered.`);
        } else {
          this.error.set(err.error?.detail ?? 'Registration failed. Is the API running?');
        }
      },
    });
  }
}
