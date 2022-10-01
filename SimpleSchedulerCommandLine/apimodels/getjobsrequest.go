package apimodels

type GetJobsRequest struct {
	WorkerID    *int64  `json:"WorkerID"`
	WorkerName  *string `json:"WorkerName"`
	StatusCode  *string `json:"StatusCode"`
	RowsPerPage int32   `json:"RowsPerPage"`
	PageNumber  int32   `json:"PageNumber"`
	OverdueOnly bool    `json:"OverdueOnly"`
}

func NewGetJobsRequest(wid *int64, wname *string, stat *string, rowsPerPage int32, pgnum int32, overdueOnly bool) *GetJobsRequest {
	return &GetJobsRequest{
		WorkerID:    wid,
		WorkerName:  wname,
		StatusCode:  stat,
		RowsPerPage: rowsPerPage,
		PageNumber:  pgnum,
		OverdueOnly: overdueOnly,
	}
}
