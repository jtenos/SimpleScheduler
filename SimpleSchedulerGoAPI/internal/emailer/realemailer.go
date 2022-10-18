package emailer

import (
	"fmt"
	"net/smtp"
)

type realEmailer struct {
	cfg emailConfig
}

func (e realEmailer) SendEmailToAdmin(subj string, bodyHTML string) (err error) {
	recipients := []string{e.cfg.admin}
	return SendEmail(recipients, subj, bodyHTML)
}

func (e realEmailer) SendEmail(recipients []string, subj string, bodyHTML string) (err error) {
	addr := fmt.Sprintf("%s:%d", e.cfg.host, e.cfg.port)

	subjLine := fmt.Sprintf("Subject: %s\r\n", subj)
	mimeLines := "MIME-version: 1.0;\r\nContent-Type: text/html; charset=\"UTF-8\";\r\n\r\n"
	contents := subjLine + mimeLines + bodyHTML

	msg := []byte(contents)

	auth := smtp.PlainAuth("", e.cfg.user, e.cfg.pwd, e.cfg.host)
	err = smtp.SendMail(addr, auth, e.cfg.from, recipients, msg)
	return
}
