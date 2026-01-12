import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../auth/auth-service';
import { NgOptimizedImage } from '@angular/common';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  styleUrl: './login.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgOptimizedImage],
})
export class Login implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isMockMode = environment.auth.useMock;

  ngOnInit(): void {
    // In mock mode, if already authenticated, redirect immediately to dashboard
    if (this.isMockMode && this.authService.isLoggedIn()) {
      this.router.navigate(['/dashboard']);
    }
  }

  login(): void {
    this.authService.login();

    // In mock mode, login is instant - navigate immediately
    if (this.isMockMode) {
      this.router.navigate(['/dashboard']);
    }
  }
}
