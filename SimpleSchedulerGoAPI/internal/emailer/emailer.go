package emailer

import "fmt"

type emailer interface {
	SendEmailToAdmin(subj string, bodyHTML string) error
	SendEmail(recipients []string, subj string, bodyHTML string) error
}

type emailConfig struct {
	port  int
	from  string
	admin string
	host  string
	user  string
	pwd   string
}

var emailFolder string
var emailCfg emailConfig

// Execute this to route messages to a folder with text files rather than actual email,
// for testing purposes.
func SetEmailFolder(fol string) {
	emailFolder = fol
}

// Execute this to route messages to real email.
func SetEmailConfiguration(port int, from string, admin string, host string, user string, pwd string) {
	emailCfg = emailConfig{port, from, admin, host, user, pwd}
}

func getEmailer() (e emailer, err error) {
	if len(emailFolder) > 0 {
		e = logFileEmailer{emailFolder}
	} else if len(emailCfg.host) > 0 {
		e = realEmailer{emailCfg}
	} else {
		err = fmt.Errorf("you must either have EmailFolder or MailSettings in your configuration")
	}
	return
}

func SendEmailToAdmin(subj string, bodyHTML string) (err error) {
	e, err := getEmailer()
	if err != nil {
		return
	}
	e.SendEmailToAdmin(subj, bodyHTML)
	return
}

func SendEmail(recipients []string, subj string, bodyHTML string) (err error) {
	e, err := getEmailer()
	if err != nil {
		return
	}
	e.SendEmail(recipients, subj, bodyHTML)
	return
}
