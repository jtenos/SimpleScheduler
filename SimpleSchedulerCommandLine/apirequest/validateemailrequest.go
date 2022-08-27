package apirequest

type ValidateEmailRequest struct {
	ValidationCode string `json:"validationCode"`
}

func NewValidateEmailRequest(validationCode string) *ValidateEmailRequest {
	return &ValidateEmailRequest{
		ValidationCode: validationCode,
	}
}
