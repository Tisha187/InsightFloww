import { Auth } from './core/services/auth';
import { Component, inject, signal } from '@angular/core';
import {
  Router,
  RouterLink,
  RouterLinkActive,
  RouterOutlet,
  NavigationEnd,
} from '@angular/router';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private readonly router = inject(Router);
  private readonly auth = inject(Auth);
  protected readonly menuOpen = signal(false);
  protected readonly isAuthPage = signal(false);
  accountMenuOpen = false;
  constructor() {
    this.updateAuthPage();

    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => this.updateAuthPage());
  }

  private updateAuthPage(): void {
    const path = this.router.url.split('?')[0].split('#')[0];

    this.isAuthPage.set(
      path === '/' ||
      path === '/login' ||
      path === '/register'
    );
  }

  protected toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  protected closeMenu(): void {
    this.menuOpen.set(false);
  }

  get isLoggedIn(): boolean {
    return this.auth.isLoggedIn();
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigate(['/']);
  }

 
  get currentUser() {
    return this.auth.getCurrentUser();
  }

  get userName(): string {
    return this.currentUser?.userName ?? 'User';
  }

  get userInitial(): string {
    return this.userName.charAt(0).toUpperCase();
  }

  toggleAccountMenu(): void {
    this.accountMenuOpen = !this.accountMenuOpen;
  }

}
