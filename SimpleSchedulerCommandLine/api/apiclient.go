package api

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
)

type ApiClient struct {
	baseUrl string
}

func NewApiClient(baseUrl string) *ApiClient {
	return &ApiClient{
		baseUrl: baseUrl,
	}
}

func (api *ApiClient) Post(url string, postObj any, resultObj any) error {
	url = fmt.Sprintf("%s/%s", api.baseUrl, url)

	reqBody, err := json.Marshal(postObj)
	if err != nil {
		return err
	}

	res, err := http.Post(url, "application/json", bytes.NewBuffer(reqBody))
	if err != nil {
		return err
	}
	defer res.Body.Close()

	body, err := io.ReadAll(res.Body)
	if err != nil {
		return err
	}

	json.Unmarshal(body, resultObj)

	return nil
}
