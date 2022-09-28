package job

import (
	"context"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ctxhelper"
)

func Execute(ctx context.Context) {
	verb := ctxhelper.GetVerb(ctx)

	switch verb {
	case "list":
		list(ctx)
	case "run":
		run(ctx)
	case "details":
		details(ctx)
	}
}

func list(ctx context.Context) {
	//	list --status ERR --worker 123 --workername "Some"

}

func run(ctx context.Context) {
	//	run --worker 123
	//
}

func details(ctx context.Context) {
	//details --id 123456

}
