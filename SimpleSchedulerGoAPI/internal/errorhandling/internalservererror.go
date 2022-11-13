package errorhandling

import "net/http"

type InternalServerError struct {
	message string
}

func (ise InternalServerError) StatusCode() int {
	return http.StatusInternalServerError
}

func NewInternalServerError(message string) InternalServerError {
	return InternalServerError{message}
}

func (bre InternalServerError) Error() string {
	return bre.message
}
