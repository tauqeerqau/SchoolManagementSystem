import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StudentService } from './student.service';
import { Student } from './student.model';

@Component({
  selector: 'app-students-list',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './students-list.component.html',
  styleUrl: './students-list.component.css',
})
export class StudentsListComponent implements OnInit {
  private readonly studentService = inject(StudentService);

  readonly students = signal<Student[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal('');

  ngOnInit(): void {
    this.studentService.getStudents().subscribe({
      next: (students) => {
        this.students.set(students);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(error.status
          ? `Could not load students (HTTP ${error.status}). Please try again.`
          : 'Could not connect to the API. Check that the backend is running and its HTTPS certificate is trusted.');
        this.loading.set(false);
      },
    });
  }
}
