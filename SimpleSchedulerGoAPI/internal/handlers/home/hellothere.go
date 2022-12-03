package home

import (
	"encoding/json"
	"net/http"
)

type HelloThereHandler struct{}

func NewHelloThereHandler() *HelloThereHandler {
	return &HelloThereHandler{}
}

type helloThereReply struct {
	Message string `json:"message"`
}

func (h *HelloThereHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	json.NewEncoder(w).Encode(helloThereReply{
		Message: "Howdy",
	})
}