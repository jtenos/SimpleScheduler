package ui

type menuItem struct {
	text   string
	action func()
}

func newMenuItem(text string, action func()) *menuItem {
	return &menuItem{
		text:   text,
		action: action,
	}
}

func (mi *menuItem) execute() {
	mi.action()
}
