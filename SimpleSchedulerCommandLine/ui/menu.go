package ui

import (
	"fmt"
)

type menu struct {
	title      string
	items      []*menuItem
	exitAction func()
}

func newMenu(title string, items []*menuItem, exitAction func()) *menu {
	return &menu{
		title:      title,
		items:      items,
		exitAction: exitAction,
	}
}

func (m *menu) show() {
	for {
		fmt.Printf("\n*** %s ***\n\n", m.title)
		for i, elem := range m.items {
			fmt.Printf("%3d: %s\n", i+1, elem.text)
		}
		fmt.Println("")
		fmt.Print("Make a selection (ENTER to exit): ")
		sel := readFromConsole()
		for i, elem := range m.items {
			if sel == fmt.Sprint(i+1) {
				elem.action()
				return
			}
		}
		if len(sel) == 0 {
			m.exitAction()
			return
		} else {
			fmt.Println("\nInvalid selection, please try again")
			continue
		}
	}
}
