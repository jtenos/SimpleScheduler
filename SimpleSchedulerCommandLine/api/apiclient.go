package api

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"os"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ctxhelper"
)

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

	jwt := ctxhelper.GetToken(ctx)

	if len(jwt) > 0 {
		req.Header.Add("Authorization", fmt.Sprintf("Bearer %s", jwt))
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

	if res.StatusCode == 401 {
		fmt.Println("Token is missing or expired. To log in again:")
		fmt.Println("sched user login --email test@example.com")
		os.Exit(1)
	}

	if res.StatusCode != 200 {
		return fmt.Errorf("hello error code %v: %s\n%s", res.StatusCode, res.Status, body)
	}

	err = json.Unmarshal(body, resultObj)
	if err != nil {
		fmt.Printf("%s\n", err)
	}

	return nil
}
