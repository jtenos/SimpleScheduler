package apirequest

type SubmitEmailRequest struct {
	EmailAddress string `json:"emailAddress"`
}

func NewSubmitEmailRequest(email string) *SubmitEmailRequest {
	return &SubmitEmailRequest{
		EmailAddress: email,
	}
}
