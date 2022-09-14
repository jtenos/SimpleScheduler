package apimodels

type GetWorkerRequest struct {
	ID int64 `json:"id"`
}

func NewGetWorkerRequest(id int64) *GetWorkerRequest {
	return &GetWorkerRequest{
		ID: id,
	}
}
