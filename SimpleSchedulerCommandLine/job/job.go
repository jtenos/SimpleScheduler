package job

import (
	"context"
	"os"
)

func Execute(ctx context.Context) {
	verb := os.Args[2]

	switch verb {
	case "list":
		list()
	case "cancel":
		cancel()
	case "details":
		details()
	}
}

func list() {

}

func cancel() {

}

func details() {

}
