using Serilog.Core;
using Serilog.Events;
using SimpleSchedulerEmail;

namespace SimpleSchedulerSerilogEmail;

public class EmailSink
    : ILogEventSink
{
    private readonly IEmailer _emailer;
    private readonly IFormatProvider _formatProvider;

    public EmailSink(IEmailer emailer, IFormatProvider formatProvider)
    {
        _emailer = emailer;
        _formatProvider = formatProvider;
    }

    void ILogEventSink.Emit(LogEvent logEvent)
    {
        const string SUBJECT = "Unhandled error";
        string body = logEvent.RenderMessage(_formatProvider);

        _emailer.SendEmailToAdminAsync(SUBJECT, body)
            .Wait();
    }
}
