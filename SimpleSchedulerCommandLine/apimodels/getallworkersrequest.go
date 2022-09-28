package apimodels

type GetAllWorkersRequest struct {
	WorkerName    string `json:"WorkerName"`
	DirectoryName string `json:"DirectoryName"`
	Executable    string `json:"Executable"`
	ActiveOnly    bool   `json:"ActiveOnly"`
	InactiveOnly  bool   `json:"InactiveOnly"`
}

func NewGetAllWorkersRequest(name string, dir string, exe string, activeOnly bool, inactiveOnly bool) *GetAllWorkersRequest {
	return &GetAllWorkersRequest{
		WorkerName:    name,
		DirectoryName: dir,
		Executable:    exe,
		ActiveOnly:    activeOnly,
		InactiveOnly:  inactiveOnly,
	}
}
