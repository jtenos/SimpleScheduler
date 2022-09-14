package api

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
)

type ApiClient struct {
	baseUrl  string
	jwtToken string
}

func NewApiClient(baseUrl string) *ApiClient {
	return &ApiClient{
		baseUrl: baseUrl,
	}
}

func (api *ApiClient) SetJwtToken(token string) {
	api.jwtToken = token
}

func (api *ApiClient) Post(url string, postObj any, resultObj any) error {
	url = fmt.Sprintf("%s/%s", api.baseUrl, url)

	reqBody, err := json.Marshal(postObj)
	if err != nil {
		return err
	}

	client := &http.Client{}

	req, err := http.NewRequest("POST", url, bytes.NewBuffer(reqBody))
	if err != nil {
		return err
	}

	req.Header.Add("Content-Type", "application/json;charset=utf-8")

	if len(api.jwtToken) > 0 {
		req.Header.Add("Authorization", fmt.Sprintf("Bearer %s", api.jwtToken))
	}

	res, err := client.Do(req)

	if err != nil {
		return err
	}
	defer res.Body.Close()

	body, err := io.ReadAll(res.Body)
	if err != nil {
		return err
	}

	if res.StatusCode != 200 {
		return fmt.Errorf("error code %v: %s\n%s", res.StatusCode, res.Status, body)
	}

	err = json.Unmarshal(body, resultObj)
	if err != nil {
		fmt.Printf("%s\n", err)
	}

	return nil
}
