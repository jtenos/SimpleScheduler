package config

import (
	"encoding/json"
	"log"
	"os"
	"path/filepath"
)

type MailSettings struct {
	EmailFrom  string `json:"emailFrom"`
	AdminEmail string `json:"adminEmail"`
	Host       string `json:"host"`
	UserName   string `json:"userName"`
	Password   string `json:"password"`
}

type Jwt struct {
	Key      string `json:"key"`
	Issuer   string `json:"issuer"`
	Audience string `json:"audience"`
}

type Configuration struct {
	ConnectionString      string       `json:"connectionString"`
	PathBase              string       `json:"pathBase"`
	EmailFolder           string       `json:"emailFolder"`
	MailSettings          MailSettings `json:"mailSettings"`
	WorkerPath            string       `json:"workerPath"`
	WebUrl                string       `json:"webUrl"`
	EnvironmentName       string       `json:"environmentName"`
	Jwt                   Jwt          `json:"jwt"`
	InternalSecretAuthKey string       `json:"internalSecretAuthKey"`
	AllowLoginDropdown    bool         `json:"allowLoginDropdown"`
	TerminalLogLevel      string       `json:"terminalLogLevel"`
	FileLogLevel          string       `json:"fileLogLevel"`
	LogFileName           string       `json:"logFileName"`
}

func LoadConfig() *Configuration {
	exec, err := os.Executable()
	if err != nil {
		log.Fatalf("error loading os.Executable: %v", err.Error())
		return nil
	}
	execPath := filepath.Dir(exec)
	confFile := filepath.Join(execPath, "conf.json")

	file, err := os.Open(confFile)
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
