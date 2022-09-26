package worker

import (
	"context"
	"os"
)

func Execute(ctx context.Context) {
	verb := os.Args[2]

	switch verb {
	case "list":
		list()
	case "edit":
		edit()
	case "show":
		show()
	case "run":
		run()
	}
}

func list() {

}

func edit() {

}

func show() {

}

func run() {

}
