package userdir

import (
	"os"
	"path/filepath"
)

func getSchedDir() (string, error) {
	homeDir, err := os.UserHomeDir()
	if err != nil {
		return "", err
	}
	schedDir := filepath.Join(homeDir, ".simple-scheduler")
	_, err = os.Stat(schedDir)
	if os.IsNotExist(err) {
		err = os.Mkdir(schedDir, os.ModePerm)
	}

	if err != nil {
		return "", err
	}
	return schedDir, nil
}

func getTokenFileName() (string, error) {
	schedDir, err := getSchedDir()
	if err != nil {
		return "", err
	}
	fn := filepath.Join(schedDir, "token.txt")
	return fn, nil
}

func ReadToken() (string, error) {
	fn, err := getTokenFileName()
	if err != nil {
		return "", err
	}
	f, err := os.ReadFile(fn)
	if os.IsNotExist(err) {
		return "", nil
	}
	if err != nil {
		return "", err
	}
	return string(f), nil
}

func WriteToken(token string) error {
	fn, err := getTokenFileName()
	if err != nil {
		return err
	}
	f, err := os.Create(fn)
	if err != nil {
		return err
	}
	defer f.Close()
	_, err = f.WriteString(token)
	if err != nil {
		return err
	}
	return nil
}
