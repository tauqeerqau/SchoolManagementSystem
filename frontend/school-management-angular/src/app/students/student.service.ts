import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Student } from './student.model';
import { StudentCreateRequest } from './student-create-request.model';

@Injectable({ providedIn: 'root' })
export class StudentService {
  private readonly http = inject(HttpClient);
  private readonly studentsUrl = 'https://localhost:7149/api/students';

  getStudents(): Observable<Student[]> {
    return this.http.get<Student[]>(this.studentsUrl);
  }

  getStudentById(id: number): Observable<Student> {
    return this.http.get<Student>(`${this.studentsUrl}/${id}`);
  }

  createStudent(request: StudentCreateRequest): Observable<Student> {
    return this.http.post<Student>(this.studentsUrl, request);
  }
}
