package schedule

import (
	"context"
	"os"
)

func Execute(ctx context.Context) {
	verb := os.Args[2]

	switch verb {
	case "add":
		add()
	case "delete":
		delete()
	case "edit":
		edit()
	}
}

func add() {

}

func delete() {

}

func edit() {

}
