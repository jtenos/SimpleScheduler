package api

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"os"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ctxutil"
)

func Post(ctx context.Context, url string, postObj any, resultObj any) error {

	verbose := ctxutil.GetVerbose(ctx)

	baseUrl := ctxutil.GetApiUrl(ctx)
	url = fmt.Sprintf("%s/%s", baseUrl, url)

	if verbose {
		fmt.Printf("URL: %s\n", url)
	}

	reqBody, err := json.Marshal(postObj)
	if err != nil {
		return err
	}

	if verbose {
		fmt.Printf("POST Body: %s\n", reqBody)
	}

	client := &http.Client{}

	req, err := http.NewRequest("POST", url, bytes.NewBuffer(reqBody))
	if err != nil {
		return err
	}

	req.Header.Add("Content-Type", "application/json;charset=utf-8")

	jwt := ctxutil.GetToken(ctx)

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

	if verbose {
		fmt.Printf("Response: %s\n", body)
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
