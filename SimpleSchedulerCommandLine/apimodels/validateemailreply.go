package apimodels

type ValidateEmailReply struct {
	JwtToken string `json:"jwtToken"`
}

func NewValidateEmailReply() *ValidateEmailReply {
	return &ValidateEmailReply{}
}
