package apimodels

type GetDetailedMessageRequest struct {
	ID int64 `json:"ID"`
}

func NewGetDetailedMessageRequest(id int64) *GetDetailedMessageRequest {
	return &GetDetailedMessageRequest{
		ID: id,
	}
}
