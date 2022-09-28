package token

import (
	"context"
	"fmt"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ctxhelper"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ui"
	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/userdir"
)

func Execute(ctx context.Context) {
	verb := ctxhelper.GetVerb(ctx)
	switch verb {
	case "show":
		token, err := userdir.ReadToken()
		if err != nil {
			ui.WriteFatalf("Error reading token: %s", err.Error())
		}
		fmt.Println(token)
	default:
		ui.WriteFatalf("Invalid verb: Must be 'show'")
	}
}
