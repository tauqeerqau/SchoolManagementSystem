import { Routes } from '@angular/router';
import { AddStudentComponent } from './students/add-student.component';
import { StudentDetailsComponent } from './students/student-details.component';
import { StudentsListComponent } from './students/students-list.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'students' },
  { path: 'students/new', component: AddStudentComponent },
  { path: 'students', component: StudentsListComponent },
  { path: 'students/:id', component: StudentDetailsComponent },
];
