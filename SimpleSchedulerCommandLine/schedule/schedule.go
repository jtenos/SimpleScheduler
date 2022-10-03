package schedule

import (
	"context"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ctxutil"
)

func Execute(ctx context.Context) {
	verb := ctxutil.GetVerb(ctx)

	switch verb {
	case "list":
		list(ctx)
	}
}

func list(ctx context.Context) {
	//list --worker 123

}
