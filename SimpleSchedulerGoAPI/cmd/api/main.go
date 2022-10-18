package main

import (
	"context"
	"fmt"
	"log"
	"net/http"
	"os"
	"time"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/config"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerGoAPI/internal/emailer"
)

var conf *config.Configuration

func main() {
	ctx := context.Background()
	conf = config.LoadConfig()

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

	r := newRouter(ctx, conf)
	port, ok := os.LookupEnv("PORT")
	if !ok {
		port = "8080"
	}

	addr := fmt.Sprintf(":%s", port)
	server := http.Server{
		Addr:         addr,
		Handler:      r,
		ReadTimeout:  5 * time.Second,
		WriteTimeout: 5 * time.Second,
		IdleTimeout:  5 * time.Second,
	}
	log.Println("main: running server on port", port)
	log.Fatal(server.ListenAndServe())
}
