package main

import (
	"context"
	"fmt"
	"log"
	"net/http"
	"os"
	"time"

	"github.com/jtenos/simplescheduler/internal/config"
	"github.com/jtenos/simplescheduler/internal/ctxutil"
	"github.com/jtenos/simplescheduler/internal/data"
	"github.com/jtenos/simplescheduler/internal/emailer"
)

var conf *config.Configuration

func main() {
	ctx := context.Background()
	conf = config.LoadConfig()

	ctxutil.SetAllowLoginDropDown(&ctx, conf.AllowLoginDropdown)
	ctxutil.SetApiUrl(&ctx, conf.ApiUrl)
	ctxutil.SetDBFileName(&ctx, conf.DBFileName)
	ctxutil.SetWorkerPath(&ctx, conf.WorkerPath)

	if len(conf.EmailFolder) > 0 {
		emailer.SetEmailFolder(conf.EmailFolder)
	} else if len(conf.MailSettings.Host) > 0 {
		emailer.SetEmailConfiguration(conf.MailSettings.Port, conf.MailSettings.EmailFrom,
			conf.MailSettings.AdminEmail, conf.MailSettings.Host, conf.MailSettings.UserName,
			conf.MailSettings.Password)
	} else {
		log.Fatal("You must have MailSettings or EmailFolder in your configuration")
		return
	}

	if err := data.InitDB(ctx); err != nil {
		log.Fatalf("Error initializing database: %v", err)
	}

	mux := newMux(ctx, newMuxParms(conf.ApiUrl, []byte(conf.Jwt.Key), conf.EnvironmentName, conf.WorkerPath))
	port, ok := os.LookupEnv("PORT")
	if !ok {
		port = "8080"
	}

	addr := fmt.Sprintf(":%s", port)
	server := http.Server{
		Addr:         addr,
		Handler:      mux,
		ReadTimeout:  5 * time.Second,
		WriteTimeout: 5 * time.Second,
		IdleTimeout:  5 * time.Second,
	}
	log.Println("main: running server on port", port)
	log.Fatal(server.ListenAndServe())
}
