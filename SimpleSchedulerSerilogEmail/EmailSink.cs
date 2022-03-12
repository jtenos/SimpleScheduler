using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using SimpleSchedulerEmail;
using System.Diagnostics;

namespace SimpleSchedulerSerilogEmail;

public class EmailSink
    : ILogEventSink
{
    private ITextFormatter _textFormatter;

    private static IEmailer _emailer = default!;

    public EmailSink(ITextFormatter textFormatter)
    {
        _textFormatter = textFormatter;
    }

    void ILogEventSink.Emit(LogEvent logEvent)
    {
        if (_emailer == null)
        {
            Trace.WriteLine("You need to call EmailSink.SetEmailer to set the emailer in order to receive critical emails");
            return;
        }
        const string SUBJECT = "Unhandled error";
        string body = logEvent.RenderMessage();

        StringWriter payload = new();
        _textFormatter.Format(logEvent, payload);

        _emailer.SendEmailToAdmin(SUBJECT, payload.ToString());
    }

    public static void SetEmailer(IEmailer emailer)
    {
        _emailer = emailer;
    }
}
