package apimodels

type GetDetailedMessageReply struct {
	DetailedMessage *string
}

func NewGetDetailedMessageReply() *GetDetailedMessageReply {
	return &GetDetailedMessageReply{}
}
