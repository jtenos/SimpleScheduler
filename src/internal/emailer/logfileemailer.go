package emailer

import (
	"errors"
	"fmt"
	"log"
	"os"
	"path/filepath"
	"strings"
	"time"

	"github.com/google/uuid"
)

type logFileEmailer struct {
	emailFolder string
}

func (e logFileEmailer) SendEmailToAdmin(subj string, bodyHTML string) (err error) {
	recipients := []string{"(ADMIN)"}
	return e.SendEmail(recipients, subj, bodyHTML)
}

func (e logFileEmailer) SendEmail(recipients []string, subj string, bodyHTML string) (err error) {

	bodyHTML = strings.ReplaceAll(bodyHTML, "<br>", "\n")

	if _, err = os.Stat(e.emailFolder); errors.Is(err, os.ErrNotExist) {
		err = os.Mkdir(e.emailFolder, os.ModePerm)
		if err != nil {
			return
		}
	}

	fn := filepath.Join(e.emailFolder, fmt.Sprintf("Message_%s_%s.txt",
		time.Now().Format("20060102-150400"), uuid.NewString()))
	log.Printf("Writing output to %s", fn)
	f, err := os.OpenFile(fn, os.O_CREATE|os.O_WRONLY, os.ModeAppend)
	if err != nil {
		return
	}
	defer f.Close()
	fmt.Fprint(f, "To Addresses:\n")
	for i := range recipients {
		fmt.Fprintf(f, "  %s\n", recipients[i])
	}
	fmt.Fprintf(f, "Subject: %s\n", subj)
	fmt.Fprintln(f)
	fmt.Fprintln(f, bodyHTML)
	return
}
