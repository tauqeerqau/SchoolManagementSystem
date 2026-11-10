import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { StudentCreateRequest } from './student-create-request.model';
import { StudentService } from './student.service';

@Component({
  selector: 'app-add-student',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './add-student.component.html',
  styleUrl: './add-student.component.css',
})
export class AddStudentComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly studentService = inject(StudentService);
  private readonly router = inject(Router);

  readonly submitted = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  readonly form = this.formBuilder.nonNullable.group({
    firstName: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    dateOfBirth: ['', Validators.required],
    enrollmentDate: ['', Validators.required],
    isActive: ['', Validators.required],
  });

  submit(): void {
    this.submitted.set(true);
    this.errorMessage.set('');

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const values = this.form.getRawValue();
    const request: StudentCreateRequest = {
      firstName: values.firstName.trim(),
      lastName: values.lastName.trim(),
      email: values.email.trim(),
      dateOfBirth: values.dateOfBirth,
      enrollmentDate: values.enrollmentDate,
      isActive: values.isActive === 'true',
    };

    this.saving.set(true);
    this.studentService.createStudent(request).subscribe({
      next: () => {
        this.successMessage.set('Student created successfully. Returning to the student list…');
        window.setTimeout(() => {
          void this.router.navigateByUrl('/students');
        }, 900);
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        if (error.status === 409) {
          this.errorMessage.set('A student with this email address already exists.');
        } else if (error.status === 400) {
          this.errorMessage.set('Some information was not accepted. Please review the fields and try again.');
        } else {
          this.errorMessage.set('The student could not be saved. Please try again.');
        }
      },
    });
  }
}
