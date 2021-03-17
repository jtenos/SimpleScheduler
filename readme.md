In order to run this, you'll need your configuration in environment variables in your system, rather than in an appsettings file.

Environment Variables:

SCHEDULER_EMAIL_FROM
Email address for the sender - this can be a real address or a "noreply" address - this is required

SCHEDULER_ADMIN_EMAIL
Email address for the admin - this address receives failure messages and other administration emails - this is required

SCHEDULER_SMTP_SECURE
Value is 1 for secure, otherwise not secure

SCHEDULER_SMTP_PORT
Port number, or if missing then it uses the default port

SCHEDULER_SMTP_HOST
The SMTP server - this is required

SCHEDULER_SMTP_USERNAME
The username for SMTP - optional if you are not using a login/password for SMTP

SCHEDULER_SMTP_PASSWORD
The password for SMTP - optional if you are not using a login/password for SMTP
