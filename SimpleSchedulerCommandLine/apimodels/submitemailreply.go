package apimodels

type SubmitEmailReply struct {
	Success bool `json:"success"`
}

func NewSubmitEmailReply() *SubmitEmailReply {
	return &SubmitEmailReply{}
}
