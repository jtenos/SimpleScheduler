package config

import (
	"encoding/json"
	"log"
	"os"
)

type Configuration struct {
	ApiUrl string `json:"apiUrl"`
}

func LoadConfig() *Configuration {
	file, err := os.Open("conf.json")
	if err != nil {
		log.Fatal(err)
	}
	defer file.Close()
	decoder := json.NewDecoder(file)
	config := &Configuration{}
	err = decoder.Decode(config)
	if err != nil {
		log.Fatal(err)
	}
	return config
}
