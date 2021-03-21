import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ReactiveFormsModule } from "@angular/forms";
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

import { SemicolonToNewLinePipe } from './pipes/semicolon-to-new-line.pipe';
import { CustomInterceptor } from './custom-interceptor';

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

    SemicolonToNewLinePipe
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule
  ],
  providers: [{
    provide: HTTP_INTERCEPTORS,
    useClass: CustomInterceptor,
    multi: true
  }],
  bootstrap: [AppComponent]
})
export class AppModule { }
