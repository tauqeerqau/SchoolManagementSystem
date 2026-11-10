import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Student } from './student.model';
import { StudentService } from './student.service';

@Component({
  selector: 'app-student-details',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './student-details.component.html',
  styleUrl: './student-details.component.css',
})
export class StudentDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly studentService = inject(StudentService);

  readonly student = signal<Student | null>(null);
  readonly loading = signal(true);
  readonly errorMessage = signal('');
  readonly notFound = signal(false);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (!Number.isInteger(id) || id <= 0) {
      this.loading.set(false);
      this.errorMessage.set('The student ID must be a positive whole number.');
      return;
    }

    this.studentService.getStudentById(id).subscribe({
      next: (student) => {
        this.student.set(student);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        if (error.status === 404) {
          this.notFound.set(true);
        } else {
          this.errorMessage.set(error.status
            ? `Could not load this student (HTTP ${error.status}). Please try again.`
            : 'Could not connect to the API. Check that the backend is running and its HTTPS certificate is trusted.');
        }
        this.loading.set(false);
      },
    });
  }
}
