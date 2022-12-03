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

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/config"
	homeHandlers "github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/handlers/home"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/handlers/schedules"
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
	log.Println("Writing header from statusRecorder.WriteHeader", status)
	r.ResponseWriter.WriteHeader(status)
}

func newMux(ctx context.Context, conf *config.Configuration) *http.ServeMux {

	mux := http.NewServeMux()

	jwtKey, err := hex.DecodeString(conf.Jwt.Key)
	if err != nil {
		log.Fatalf("error decoding JWT key")
	}

	// HOME
	setHandling(mux, "/home/getUtcNow", "GET", homeHandlers.NewGetUtcNowHandler(), jwtKey)
	setHandling(mux, "/home/helloThere", "GET", homeHandlers.NewHelloThereHandler(), jwtKey)

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
	setHandlingWithoutAuth(mux, "/security/getAllUserEmails", "GET", security.NewGetAllUserEmailsHandler(ctx, conf.ConnectionString), jwtKey)
	setHandlingWithoutAuth(mux, "/security/submitEmail", "GET", security.NewSubmitEmailHandler(ctx,
		conf.ConnectionString, conf.ApiUrl, conf.EnvironmentName), jwtKey)
	setHandlingWithoutAuth(mux, "/security/validateEmail", "GET", security.NewValidateEmailHandler(ctx,
		conf.ConnectionString, jwtKey), jwtKey)
	setHandling(mux, "/security/validateToken", "GET", security.NewValidateTokenHandler(ctx, jwtKey), jwtKey)

	setHandling(mux, "/schedules/create", "POST", schedules.NewCreateHandler(ctx, conf.ConnectionString), jwtKey)
	setHandling(mux, "/schedules/delete", "POST", schedules.NewDeleteHandler(ctx, conf.ConnectionString), jwtKey)
	setHandling(mux, "/schedules/reactivate", "POST", schedules.NewReactivateHandler(ctx, conf.ConnectionString), jwtKey)

	// SCHEDULES
	/*
	   app.MapPost("/Schedules/GetAllSchedules", GetAllSchedulesAsync);
	   app.MapPost("/Schedules/GetSchedules", GetSchedulesAsync);
	   app.MapPost("/Schedules/GetSchedule", GetScheduleAsync);
	   app.MapPost("/Schedules/UpdateSchedule", UpdateScheduleAsync);

	*/

	setHandling(mux, "/workers/search", "GET", workers.NewSearchHandler(ctx, conf.ConnectionString), jwtKey)
	setHandling(mux, "/workers/create", "POST", workers.NewCreateHandler(ctx, conf.ConnectionString, conf.WorkerPath), jwtKey)
	setHandling(mux, "/workers/delete", "POST", workers.NewDeleteHandler(ctx, conf.ConnectionString), jwtKey)
	setHandling(mux, "/workers/reactivate", "POST", workers.NewReactivateHandler(ctx, conf.ConnectionString), jwtKey)
	setHandling(mux, "/workers/run", "POST", workers.NewRunHandler(ctx, conf.ConnectionString), jwtKey)
	setHandling(mux, "/workers/update", "POST", workers.NewUpdateHandler(ctx, conf.ConnectionString, conf.WorkerPath), jwtKey)

	return mux
}

func setHandling(r *http.ServeMux, path string, verb string, handler http.Handler, jwtKey []byte) {
	r.Handle(path, jsoning(authenticating(verbing(logging(handler), verb), jwtKey)))
}

func setHandlingWithoutAuth(r *http.ServeMux, path string, verb string, handler http.Handler, jwtKey []byte) {
	r.Handle(path, jsoning(verbing(logging(handler), verb)))
}

func verbing(next http.Handler, verb string) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		if r.Method != verb {
			w.Header().Set("Allow", verb)
			http.Error(w, "method not allowed", http.StatusMethodNotAllowed)
			return
		}

		next.ServeHTTP(w, r)
	})
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
