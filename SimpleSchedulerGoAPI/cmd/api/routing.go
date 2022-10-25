package main

import (
	"context"
	"encoding/hex"
	"fmt"
	"log"
	"math/rand"
	"net/http"
	"strings"
	"time"

	"github.com/gorilla/mux"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/config"
	homeHandlers "github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/handlers/home"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/handlers/security"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/handlers/workers"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/jwt"
	"golang.org/x/text/language"
	"golang.org/x/text/message"
)

type statusRecorder struct {
	http.ResponseWriter
	status int
}

func (r *statusRecorder) WriteHeader(status int) {
	r.status = status
	r.ResponseWriter.WriteHeader(status)
}

func newRouter(ctx context.Context, conf *config.Configuration) *mux.Router {

	jwtKey, err := hex.DecodeString(conf.Jwt.Key)
	if err != nil {
		log.Fatalf("error decoding JWT key")
	}

	r := mux.NewRouter()

	// HOME
	setHandling(r, "/home/getUtcNow", homeHandlers.NewGetUtcNowHandler(), jwtKey).Methods("GET")
	setHandling(r, "/home/helloThere", homeHandlers.NewHelloThereHandler(), jwtKey).Methods("GET")

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
	setHandling(r, "/security/getAllUserEmails", security.NewGetAllUserEmailsHandler(ctx, conf.ConnectionString), jwtKey).Methods("GET")
	setHandlingWithoutAuth(r, "/security/submitEmail", security.NewSubmitEmailHandler(ctx,
		conf.ConnectionString, conf.ApiUrl, conf.EnvironmentName), jwtKey).Methods("GET")
	setHandlingWithoutAuth(r, "/security/validateEmail", security.NewValidateEmailHandler(ctx,
		conf.ConnectionString, jwtKey), jwtKey).Methods("GET")
	setHandling(r, "/security/validateToken", security.NewValidateTokenHandler(ctx, jwtKey), jwtKey).Methods("GET")

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

	setHandling(r, "/workers/search", workers.NewSearchHandler(ctx, conf.ConnectionString), jwtKey).Methods("GET")
	// WORKERS
	/*
			        app.MapPost("/Workers/CreateWorker", CreateWorkerAsync);
		        app.MapPost("/Workers/DeleteWorker", DeleteWorkerAsync);
		        app.MapPost("/Workers/ReactivateWorker", ReactivateWorkerAsync);
		        app.MapPost("/Workers/RunNow", RunNowAsync);
		        app.MapPost("/Workers/UpdateWorker", UpdateWorkerAsync);

	*/

	statDir := http.Dir("./www/")
	statHandler := http.StripPrefix("/www/", http.FileServer(statDir))
	r.PathPrefix("/www/").Handler(statHandler).Methods("GET")

	return r
}

func setHandling(r *mux.Router, path string, handler http.Handler, jwtKey []byte) *mux.Route {
	return r.Handle(path, jsoning(authenticating(logging(handler), jwtKey)))
}

func setHandlingWithoutAuth(r *mux.Router, path string, handler http.Handler, jwtKey []byte) *mux.Route {
	return r.Handle(path, jsoning(logging(handler)))
}

var printer = message.NewPrinter(language.English)

func jsoning(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json; charset=utf-8")
		next.ServeHTTP(w, r)
	})
}

func authenticating(next http.Handler, jwtKey []byte) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		log.Println("Authenticating...")
		authFields := strings.Split(r.Header.Get("Authorization"), "Bearer ")

		if len(authFields) != 2 {
			log.Println("Authorization missing or invalid")
			w.WriteHeader(http.StatusUnauthorized)
			fmt.Fprintf(w, "Unauthorized\n")
			return
		}

		tokenStr := authFields[1]
		log.Printf("JWT Token: %s", tokenStr)
		email, expires, err := jwt.ReadToken(jwtKey, tokenStr)
		if err != nil {
			log.Printf("Error: %s", err.Error())
			w.WriteHeader(http.StatusBadRequest)
			fmt.Fprintf(w, "Error reading token\n")
			return
		}
		log.Printf("Email: %s", email)
		if expires.Before(time.Now()) {
			log.Printf("Token expired on %x", expires)
			w.WriteHeader(http.StatusUnauthorized)
			fmt.Fprintf(w, "Token expired\n")
			return
		}
		if len(email) == 0 {
			log.Printf("Email is empty")
			w.WriteHeader(http.StatusUnauthorized)
			fmt.Fprintf(w, "Unauthorized\n")
			return
		}

		ctx := context.WithValue(r.Context(), jwt.EmailClaimKey{}, email)
		ctx = context.WithValue(ctx, jwt.TokenExpiresKey{}, expires)
		r = r.WithContext(ctx)
		next.ServeHTTP(w, r)
	})
}

func logging(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		start := time.Now()
		reqID := rand.Intn(9000000) + 1000000

		recorder := &statusRecorder{
			ResponseWriter: w,
			status:         200,
		}

		req := fmt.Sprintf("%s %s", r.Method, r.URL)
		log.Printf("%d: >>> %s", reqID, req)

		next.ServeHTTP(recorder, r)
		us := time.Since(start).Microseconds()
		var msg string
		if us < 1000 {
			msg = fmt.Sprintf("%d: %d %s completed in %sμs", reqID, recorder.status, req, printer.Sprint(us))
		} else {
			msg = fmt.Sprintf("%d: %d %s completed in %sms", reqID, recorder.status, req, printer.Sprint(us/1000))
		}

		log.Println(msg)
	})
}
