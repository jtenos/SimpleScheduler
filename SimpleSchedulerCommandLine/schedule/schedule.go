package schedule

import (
	"context"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ctxhelper"
)

func Execute(ctx context.Context) {
	verb := ctxhelper.GetVerb(ctx)

	switch verb {
	case "list":
		list(ctx)
	}
}

func list(ctx context.Context) {
	//list --worker 123

}
