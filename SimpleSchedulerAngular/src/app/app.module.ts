import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { AppRoutingModule } from './app-routing.module';

import { AppComponent } from './components/app/app.component';
import { EditWorkerComponent } from './components/edit-worker/edit-worker.component';
import { HomeComponent } from './components/home/home.component';
import { JobsComponent } from './components/jobs/jobs.component';
import { LoginComponent } from "./components/login/login.component";
import { NavbarComponent } from './components/navbar/navbar.component';
import { SchedulesComponent } from './components/schedules/schedules.component';
import { WorkersComponent } from './components/workers/workers.component';
import { WorkerTableComponent } from './components/worker-table/worker-table.component';

import { CustomInterceptor } from './custom-interceptor';
import { ScheduleTableComponent } from './components/schedule-table/schedule-table.component';
import { EditScheduleComponent } from './components/edit-schedule/edit-schedule.component';
import { JobTableComponent } from './components/job-table/job-table.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from "@angular/material/select";
import { MatTableModule } from "@angular/material/table";
import { MatToolbarModule } from "@angular/material/toolbar";

@NgModule({
    declarations: [
        AppComponent,
        EditWorkerComponent,
        HomeComponent,
        JobsComponent,
        LoginComponent,
        NavbarComponent,
        SchedulesComponent,
        WorkersComponent,
        WorkerTableComponent,

        ScheduleTableComponent,

        EditScheduleComponent,

        JobTableComponent
    ],
    imports: [
        BrowserModule,
        AppRoutingModule,
        HttpClientModule,
        FormsModule,
        ReactiveFormsModule,
        BrowserAnimationsModule,

        MatButtonModule,
        MatCardModule,
        MatIconModule,
        MatInputModule,
        MatSelectModule,
        MatTableModule,
        MatToolbarModule
    ],
    providers: [{
        provide: HTTP_INTERCEPTORS,
        useClass: CustomInterceptor,
        multi: true
    }],
    bootstrap: [AppComponent]
})
export class AppModule { }
