import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule } from "@angular/forms";

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './components/shared/app/app.component';
import { WorkersComponent } from './components/workers/workers/workers.component';
import { HomeComponent } from './components/home/home.component';
import { NavbarComponent } from './components/shared/navbar/navbar.component';
import { JobsComponent } from './components/jobs/jobs/jobs.component';
import { SchedulesComponent } from './components/schedules/schedules/schedules.component';
import { WorkerTableComponent } from './components/workers/worker-table/worker-table.component';
import { SemicolonToNewLinePipe } from './pipes/semicolon-to-new-line.pipe';
import { EditWorkerComponent } from './components/workers/edit-worker/edit-worker.component';

@NgModule({
  declarations: [
    AppComponent,
    WorkersComponent,
    HomeComponent,
    NavbarComponent,
    JobsComponent,
    SchedulesComponent,
    WorkerTableComponent,
    SemicolonToNewLinePipe,
    EditWorkerComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
