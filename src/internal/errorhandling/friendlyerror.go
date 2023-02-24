package errorhandling

type FriendlyError interface {
	ThisIsAFriendlyError()
	StatusCode() int
}

type FriendlyErrorBase struct{}

func (feb FriendlyErrorBase) ThisIsAFriendlyError() {}
