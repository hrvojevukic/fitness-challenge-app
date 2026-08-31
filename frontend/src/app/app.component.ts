import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';
import { ThemeService } from './services/theme.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {
  title = 'Fitness Challenge';

  private auth = inject(AuthService);
  private router = inject(Router);
  readonly theme = inject(ThemeService);

  readonly isLoggedIn = this.auth.isLoggedIn;
  readonly currentUser = this.auth.currentUser;

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
