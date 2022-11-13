package errorhandling

import "net/http"

type BadRequestError struct {
	FriendlyErrorBase
	message string
}

func (bre BadRequestError) StatusCode() int {
	return http.StatusBadRequest
}

func NewBadRequestError(message string) BadRequestError {
	return BadRequestError{
		FriendlyErrorBase: FriendlyErrorBase{},
		message:           message,
	}
}

func (bre BadRequestError) Error() string {
	return bre.message
}
