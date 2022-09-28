package config

import (
	"encoding/json"
	"os"

	"github.com/jtenos/SimpleScheduler/SimpleSchedulerCommandLine/ui"
)

type Configuration struct {
	ApiUrl string `json:"apiUrl"`
}

func LoadConfig() *Configuration {
	file, err := os.Open("conf.json")
	if err != nil {
		ui.WriteFatalf(err.Error())
	}
	defer file.Close()
	decoder := json.NewDecoder(file)
	config := &Configuration{}
	err = decoder.Decode(config)
	if err != nil {
		ui.WriteFatalf(err.Error())
	}
	return config
}
