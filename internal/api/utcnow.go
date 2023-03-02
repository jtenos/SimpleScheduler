package api

import (
	"encoding/json"
	"net/http"
	"time"

	"github.com/julienschmidt/httprouter"
)

type UtcNowHandler struct{}

func NewUtcNowHandler() *UtcNowHandler {
	return &UtcNowHandler{}
}

type getUtcNowReply struct {
	FormattedDateTime string `json:"formattedDateTime"`
}

func (h *UtcNowHandler) Get(w http.ResponseWriter, r *http.Request, _ httprouter.Params) {
	json.NewEncoder(w).Encode(getUtcNowReply{
		FormattedDateTime: time.Now().UTC().Format("Jan 02 2006, 15:04") + " (UTC)",
	})
}
