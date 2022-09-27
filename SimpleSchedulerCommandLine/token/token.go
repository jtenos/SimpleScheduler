package token

import (
	"context"
	"fmt"
	"log"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ctxhelper"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/userdir"
)

func Execute(ctx context.Context) {
	verb := ctxhelper.GetVerb(ctx)
	switch verb {
	case "show":
		token, err := userdir.ReadToken()
		if err != nil {
			log.Fatalf("Error reading token: %s", err.Error())
		}
		fmt.Println(token)
	default:
		log.Fatal("Invalid verb: Must be 'show'")
	}
}
