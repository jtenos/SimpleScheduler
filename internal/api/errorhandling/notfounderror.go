package errorhandling

import "net/http"

type NotFoundError struct {
	message string
}

func (nfe NotFoundError) StatusCode() int {
	return http.StatusNotFound
}

func NewNotFoundError(message string) NotFoundError {
	return NotFoundError{message}
}

func (nfe NotFoundError) Error() string {
	return nfe.message
}
