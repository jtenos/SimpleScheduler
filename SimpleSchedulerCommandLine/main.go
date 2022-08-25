package main

import (
	"bufio"
	"bytes"
	"encoding/json"
	"fmt"
	"io/ioutil"
	"log"
	"net/http"
	"os"
	"strings"
)

const API_URL = "http://localhost:5266" // TODO: Get this from config file

func main() {
	fmt.Print("\n*** SIMPLE SCHEDULER ***\n\n")

	fmt.Print("E-Mail Address: ")

	rdr := bufio.NewReader(os.Stdin)
	uname, _ := rdr.ReadString('\n')

	uname = strings.TrimSpace(uname)

	fmt.Println("Submitting to API, please wait...")

	url := fmt.Sprintf("%s/Login/SubmitEmail", API_URL)

	req := &submitEmailRequest{
		EmailAddress: uname,
	}
	reqBody, _ := json.Marshal(req)

	res, err := http.Post(url, "application/json", bytes.NewBuffer(reqBody))
	if err != nil {
		log.Fatal(err)
	}
	defer res.Body.Close()
	body, err := ioutil.ReadAll(res.Body)
	if err != nil {
		log.Fatal(err)
	}

	submitResp := &submitEmailReply{}
	json.Unmarshal(body, submitResp)
	fmt.Printf("Success is %v\n", submitResp.Success)
}

type submitEmailRequest struct {
	EmailAddress string `json:"emailAddress"`
}

type submitEmailReply struct {
	Success bool `json:"success"`
}
