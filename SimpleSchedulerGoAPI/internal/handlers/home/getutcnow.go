package home

import (
	"encoding/json"
	"net/http"
	"time"
)

type GetUtcNowHandler struct{}

func NewGetUtcNowHandler() *GetUtcNowHandler {
	return &GetUtcNowHandler{}
}

type getUtcNowReply struct {
	FormattedDateTime string `json:"formattedDateTime"`
}

func (h *GetUtcNowHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	json.NewEncoder(w).Encode(getUtcNowReply{
		FormattedDateTime: time.Now().UTC().Format("Jan 02 2006, 15:04") + " (UTC)",
	})
}
