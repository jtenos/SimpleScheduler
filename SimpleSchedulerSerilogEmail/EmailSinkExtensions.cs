using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Display;
using SimpleSchedulerEmail;

namespace SimpleSchedulerSerilogEmail;

public static class EmailSinkExtensions
{
    public static LoggerConfiguration Email(
        this LoggerSinkConfiguration sinkConfiguration,
        string outputTemplate,
        LogEventLevel restrictedToMinimumLevel)
    {
        MessageTemplateTextFormatter textFormatter = new (outputTemplate, formatProvider: null);

        return sinkConfiguration.Sink(
            logEventSink: new EmailSink(textFormatter),
            restrictedToMinimumLevel: restrictedToMinimumLevel);
    }
}
