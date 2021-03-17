using SchedulerEmail;

// TODO: Assign these from environment variables
string adminEmail = GetVariableValue("SCHEDULER_ADMIN_EMAIL");
await Emailer.SendEamilAsync(adminEmail, new[] { adminEmail }, "Test Subject", "Test <b>message</b>");
