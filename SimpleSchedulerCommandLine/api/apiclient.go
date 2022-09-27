package api

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"net/http"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ctxhelper"
)

var jwtToken string

func SetJwtToken(token string) {
	jwtToken = token
}

func Post(ctx context.Context, url string, postObj any, resultObj any) error {
	baseUrl := ctxhelper.GetApiUrl(ctx)
	url = fmt.Sprintf("%s/%s", baseUrl, url)

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

	if len(jwtToken) > 0 {
		req.Header.Add("Authorization", fmt.Sprintf("Bearer %s", jwtToken))
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
