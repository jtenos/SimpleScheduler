package main

import (
	"context"
	"fmt"
	"log"
	"math/rand"
	"net/http"
	"strings"
	"time"

	"github.com/jtenos/simplescheduler/internal/api"
	"github.com/jtenos/simplescheduler/internal/jwt"
	"github.com/julienschmidt/httprouter"
	"golang.org/x/text/language"
	"golang.org/x/text/message"
)

type statusRecorder struct {
	http.ResponseWriter
	status int
}

type muxParms struct {
	apiUrl     string
	jwtKey     []byte
	envName    string
	workerPath string
}

var printer = message.NewPrinter(language.English)

func newMuxParms(apiUrl string, jwtKey []byte, envName string, workerPath string) muxParms {
	return muxParms{
		apiUrl,
		jwtKey,
		envName,
		workerPath,
	}
}

func (r *statusRecorder) WriteHeader(status int) {
	r.status = status
	log.Println("Writing header from statusRecorder.WriteHeader", status)
	r.ResponseWriter.WriteHeader(status)
}

func newMux(ctx context.Context, parms muxParms) *httprouter.Router {

	mux := httprouter.New()

	utcNowHandler := api.NewUtcNowHandler()

	//////////////////////////////////////////////////////////////////////////////////////////
	// GET /utcnow
	// Get the current formatted time in UTC
	// Response: {"formattedDateTime":"Mar 02 2023, 02:39 (UTC)"}
	mux.GET("/utcnow", withoutAuth(utcNowHandler.Get))
	//////////////////////////////////////////////////////////////////////////////////////////

	userEmailHandler := api.NewUserEmailHandler(ctx)

	//////////////////////////////////////////////////////////////////////////////////////////
	// GET /useremail
	// If the allowLoginDropdown config option is set to true, else empty
	// Response: ["test@example.com","someoneelse@example.com"]
	mux.GET("/useremail", withoutAuth(userEmailHandler.Get))
	//////////////////////////////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////////////////////
	// POST /useremail
	// Start the log in process
	// Body: {"email":"test@example.com"}
	// Response: {}
	mux.POST("/useremail", withoutAuth(userEmailHandler.Post))
	//////////////////////////////////////////////////////////////////////////////////////////

	workersHandler := api.NewWorkersHandler(ctx)

	//////////////////////////////////////////////////////////////////////////////////////////
	// GET /workers?name=hello&desc=something&directory=myapp&executable=myapp&active=1&parent=34
	// Search for workers
	// Response: [{"id":1...},{"id":2...}]
	mux.GET("/workers", withAuth(workersHandler.Get, parms.jwtKey))
	//////////////////////////////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////////////////////
	// GET /workers/:id
	// Get a single worker by ID
	// Response: {
	// "id":1,"isActive":true,"workerName":"Test","detailedDescription":"Test",
	// "emailOnSuccess": "test@example.com","parentWorkerID":1,"timeoutMinutes":20
	// "directoryName":"something","executable":"something.exe","argumentValues:"-x"}
	mux.GET("/workers/:id", withAuth(workersHandler.Get, parms.jwtKey))
	//////////////////////////////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////////////////////
	// POST /workers
	// Create a new worker
	// Body: {"workerName":"the name","detailedDescription":"","emailOnSuccess":"",
	//        "parentWorkerID":22,"timeoutMinutes":20,"directoryName":"DirName",
	//        "executable":"Something.exe","argumentValues":""}
	// Response: {"workerID":1234}
	mux.POST("/workers", withAuth(workersHandler.Post, parms.jwtKey))
	//////////////////////////////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////////////////////
	// PUT /workers
	// Update an existing worker
	// Body: {"id":123,"workerName":"the name","detailedDescription":"","emailOnSuccess":"",
	//        "parentWorkerID":22,"timeoutMinutes":20,"directoryName":"DirName",
	//        "executable":"Something.exe","argumentValues":""}
	// Response: {}
	mux.PUT("/workers", withAuth(workersHandler.Put, parms.jwtKey))
	//////////////////////////////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////////////////////
	// DELETE /workers/:id
	// Delete a worker
	// Response: {}
	mux.DELETE("/workers/:id", withAuth(workersHandler.Delete, parms.jwtKey))
	//////////////////////////////////////////////////////////////////////////////////////////

	jobsHandler := api.NewJobsHandler(ctx)

	//////////////////////////////////////////////////////////////////////////////////////////
	// POST /jobs
	// Create a new job for a particular schedule
	// Body: {"scheduleID":1234,"queueDateUTC":"2023-01-02T03:04:05Z"}
	// Response: {"jobID":23456}
	//////////////////////////////////////////////////////////////////////////////////////////
	mux.POST("/jobs", withAuth(jobsHandler.Post, parms.jwtKey))
	//////////////////////////////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////////////////////
	// DELETE /jobs/:id
	// Cancel a job, but it will only cancel if it's currently in NEW status
	// Response: {}
	mux.DELETE("/jobs/:id", withAuth(jobsHandler.Delete, parms.jwtKey))
	//////////////////////////////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////

	// setHandling(mux, "/workers/run", "POST", workers.NewRunHandler(ctx, parms.connStr), jwtKey)

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
	// setHandlingWithoutAuth(mux, "/security/getAllUserEmails", "GET", security.NewGetAllUserEmailsHandler(ctx, parms.connStr), jwtKey)
	// setHandlingWithoutAuth(mux, "/security/submitEmail", "GET", security.NewSubmitEmailHandler(ctx,
	// 	parms.connStr, parms.apiUrl, parms.envName), jwtKey)
	// setHandlingWithoutAuth(mux, "/security/validateEmail", "GET", security.NewValidateEmailHandler(ctx,
	// 	parms.connStr, jwtKey), jwtKey)
	// setHandling(mux, "/security/validateToken", "GET", security.NewValidateTokenHandler(ctx, jwtKey), jwtKey)

	// setHandling(mux, "/schedules/get", "GET", schedules.NewGetHandler(ctx, parms.connStr), jwtKey)
	// setHandling(mux, "/schedules/create", "POST", schedules.NewCreateHandler(ctx, parms.connStr), jwtKey)
	// setHandling(mux, "/schedules/delete", "POST", schedules.NewDeleteHandler(ctx, parms.connStr), jwtKey)
	// setHandling(mux, "/schedules/reactivate", "POST", schedules.NewReactivateHandler(ctx, parms.connStr), jwtKey)

	// SCHEDULES
	/*
	   app.MapPost("/Schedules/GetSchedule", GetScheduleAsync);
	   app.MapPost("/Schedules/UpdateSchedule", UpdateScheduleAsync);

	*/

	return mux
}

func withoutAuth(next httprouter.Handle) httprouter.Handle {
	return corsing(jsoning(verbing(logging(func(w http.ResponseWriter, r *http.Request, parms httprouter.Params) {
		next(w, r, parms)
	}))))
}

func withAuth(next httprouter.Handle, jwtKey []byte) httprouter.Handle {
	return corsing(jsoning(authenticating(verbing(logging(func(w http.ResponseWriter, r *http.Request, parms httprouter.Params) {
		next(w, r, parms)
	})), jwtKey)))
}

// func setHandling(mux *httprouter.Router, path string, verb string, handle httprouter.Handle, jwtKey []byte) {
// 	mux.Handle(verb, path, corsing(jsoning(authenticating(verbing(logging(handle), verb), jwtKey))))
// }

// func setHandlingWithoutAuth(mux *httprouter.Router, path string, verb string, handle httprouter.Handle, jwtKey []byte) {
// 	mux.Handle(verb, path, corsing(jsoning(verbing(logging(handle)))))
// }

func corsing(next httprouter.Handle) httprouter.Handle {
	return func(w http.ResponseWriter, r *http.Request, parms httprouter.Params) {
		log.Println("Setting CORS headers")
		w.Header().Set("Access-Control-Allow-Origin", "*")
		w.Header().Set("Access-Control-Allow-Headers", "Content-Type")
		next(w, r, parms)
	}
}

func jsoning(next httprouter.Handle) httprouter.Handle {
	return func(w http.ResponseWriter, r *http.Request, parms httprouter.Params) {
		w.Header().Set("Content-Type", "application/json; charset=utf-8")
		next(w, r, parms)
	}
}

func authenticating(next httprouter.Handle, jwtKey []byte) httprouter.Handle {
	return func(w http.ResponseWriter, r *http.Request, parms httprouter.Params) {
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
		next(w, r, parms)
	}
}

func verbing(next httprouter.Handle) httprouter.Handle {
	return func(w http.ResponseWriter, r *http.Request, parms httprouter.Params) {

		if r.Method != "GET" && r.Method != "POST" && r.Method != "PUT" && r.Method != "DELETE" && r.Method != "OPTIONS" {
			http.Error(w, "method not allowed", http.StatusMethodNotAllowed)
			return
		}

		next(w, r, parms)
	}
}

func logging(next httprouter.Handle) httprouter.Handle {
	return func(w http.ResponseWriter, r *http.Request, parms httprouter.Params) {
		start := time.Now()
		reqID := rand.Intn(9000000) + 1000000

		recorder := &statusRecorder{
			ResponseWriter: w,
			status:         200,
		}

		req := fmt.Sprintf("%s %s", r.Method, r.URL)
		log.Printf("%d: >>> %s", reqID, req)

		next(recorder, r, parms)
		us := time.Since(start).Microseconds()
		var msg string
		if us < 1000 {
			msg = fmt.Sprintf("%d: %d %s completed in %sμs", reqID, recorder.status, req, printer.Sprint(us))
		} else {
			msg = fmt.Sprintf("%d: %d %s completed in %sms", reqID, recorder.status, req, printer.Sprint(us/1000))
		}

		log.Println(msg)
	}
}
