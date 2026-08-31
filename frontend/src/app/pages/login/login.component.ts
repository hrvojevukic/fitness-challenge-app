import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

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

    this.auth.login(firstName, lastName).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/log-activity';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err: HttpErrorResponse) => {
        this.submitting.set(false);
        if (err.status === 401) {
          this.error.set(`No registered user named '${firstName} ${lastName}'. Check the spelling or register first.`);
        } else {
          this.error.set(err.error?.detail ?? 'Login failed. Is the API running?');
        }
      },
    });
  }
}
