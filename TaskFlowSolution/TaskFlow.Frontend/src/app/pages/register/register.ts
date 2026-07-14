import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
@Component({
  selector: 'app-register',
  imports: [
    ReactiveFormsModule,
    RouterLink,
  ],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  isLoading = false;
  errorMessage = '';

  readonly registerForm = this.fb.nonNullable.group({
    username: [
      '',
      [
        Validators.required,
      ],
    ],
    password: [
      '',
      [
        Validators.required,
        Validators.minLength(6),
      ],
    ],
  });

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.register(this.registerForm.getRawValue()).subscribe({
      next: () => {
        this.router.navigate(['/task-lists']);
      },

      error: (error) => {
        this.isLoading = false;

        if (error.status === 401) {
          this.errorMessage = 'Invalid username or password.';
        }
        else {
          this.errorMessage = 'Unexpected error.';
        }
      },

      complete: () => {
        this.isLoading = false;
      },
    });
  }
}