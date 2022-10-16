package main

import (
	"fmt"
	"log"
	"net/http"
	"sync"
	"time"

	"github.com/gorilla/mux"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/config"
	homeHandlers "github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/handlers/home"
	"golang.org/x/text/language"
	"golang.org/x/text/message"
)

func newRouter(conf *config.Configuration) *mux.Router {
	r := mux.NewRouter()

	// HOME
	setHandling(r, "/home/getUtcNow", new(homeHandlers.GetUtcNowHandler)).Methods("GET")
	setHandling(r, "/home/helloThere", new(homeHandlers.HelloThereHandler)).Methods("GET")

	// JOBS
	/*
			        app.MapPost("/Jobs/AcknowledgeError", AcknowledgeErrorAsync);
		        app.MapPost("/Jobs/CancelJob", CancelJobAsync);
		        app.MapPost("/Jobs/CompleteJob", CompleteJobAsync);
		        app.MapPost("/Jobs/DequeueScheduledJobs", DequeueScheduledJobsAsync);
		        app.MapPost("/Jobs/GetDetailedMessage", GetDetailedMessageAsync);
		        app.MapPost("/Jobs/GetJob", GetJobAsync);
		        app.MapPost("/Jobs/GetJobs", GetJobsAsync);
		        app.MapPost("/Jobs/GetOverdueJobs", GetOverdueJobsAsync);
		        app.MapPost("/Jobs/RestartStuckJobs", RestartStuckJobsAsync);
		        app.MapPost("/Jobs/StartDueJobs", StartDueJobsAsync);
	*/

	// SECURITY
	/*
			        app.MapPost("/Login/GetAllUserEmails", GetAllUserEmailsAsync);
		        app.MapPost("/Login/IsLoggedIn", IsLoggedInAsync);
		        app.MapPost("/Login/SubmitEmail", SubmitEmailAsync);
		        app.MapPost("/Login/ValidateEmail", ValidateEmailAsync);
	*/

	// SCHEDULES
	/*
			        app.MapPost("/Schedules/CreateSchedule", CreateScheduleAsync);
		        app.MapPost("/Schedules/DeleteSchedule", DeleteScheduleAsync);
		        app.MapPost("/Schedules/GetAllSchedules", GetAllSchedulesAsync);
		        app.MapPost("/Schedules/GetSchedules", GetSchedulesAsync);
		        app.MapPost("/Schedules/GetSchedule", GetScheduleAsync);
		        app.MapPost("/Schedules/ReactivateSchedule", ReactivateScheduleAsync);
		        app.MapPost("/Schedules/UpdateSchedule", UpdateScheduleAsync);

	*/

	// WORKERS
	/*
			        app.MapPost("/Workers/CreateWorker", CreateWorkerAsync);
		        app.MapPost("/Workers/DeleteWorker", DeleteWorkerAsync);
		        app.MapPost("/Workers/GetAllWorkers", GetAllWorkersAsync);
		        app.MapPost("/Workers/GetAllActiveWorkerIDNames", GetAllActiveWorkerIDNamesAsync);
		        app.MapPost("/Workers/GetWorkers", GetWorkersAsync);
		        app.MapPost("/Workers/GetWorker", GetWorkerAsync);
		        app.MapPost("/Workers/ReactivateWorker", ReactivateWorkerAsync);
		        app.MapPost("/Workers/RunNow", RunNowAsync);
		        app.MapPost("/Workers/UpdateWorker", UpdateWorkerAsync);

	*/

	statDir := http.Dir("./www/")
	statHandler := http.StripPrefix("/www/", http.FileServer(statDir))
	r.PathPrefix("/www/").Handler(statHandler).Methods("GET")

	return r
}

func setHandling(r *mux.Router, path string, handler http.Handler) *mux.Route {
	return r.Handle(path, logging(handler))
}

var printer = message.NewPrinter(language.English)
var loggingMu = sync.Mutex{}

func logging(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		start := time.Now()
		req := fmt.Sprintf("%s %s", r.Method, r.URL)

		loggingMu.Lock()
		log.Println(req)
		loggingMu.Unlock()

		next.ServeHTTP(w, r)
		us := time.Since(start).Microseconds()
		var msg string
		if us < 1000 {
			msg = printer.Sprintf("%s completed in %dμs", req, us)
		} else {
			msg = printer.Sprintf("%s completed in %dms", req, us/1000)
		}

		loggingMu.Lock()
		log.Println(msg)
		loggingMu.Unlock()
	})
}
