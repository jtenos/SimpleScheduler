using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SimpleSchedulerApiModels;

namespace SimpleSchedulerBlazor.Client.Components.Schedules;

partial class ScheduleEdit
{
    private Guid UniqueId { get; } = Guid.NewGuid();

    private EditContext ScheduleEditContext { get; set; } = default!;

    private const string TIME = "TIME";
    private const string RECUR = "RECUR";

    private string TimeType
    {
        get
        {
            if (Schedule.TimeOfDayUTC.HasValue)
            {
                return TIME;
            }
            if (Schedule.RecurTime.HasValue)
            {
                return RECUR;
            }
            return TIME; // Default value for new schedule
        }
        set
        {
            switch (value)
            {
                case TIME:
                    Schedule.RecurTime = null;
                    Schedule.RecurBetweenStartUTC = null;
                    Schedule.RecurBetweenEndUTC = null;
                    break;
                case RECUR:
                    Schedule.TimeOfDayUTC = null;
                    break;
            }
        }
    }

    private static string? GetTimeSpanValue(TimeSpan? ts)
    {
        return ts?.ToString("hh\\:mm");
    }

    private static void SetTimeSpanValue(string? s, Action<TimeSpan?> action)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            action(null);
            return;
        }
        if (!TimeSpan.TryParseExact(s, "hh\\:mm", formatProvider: null, out TimeSpan ts))
        {
            // TODO: Error out?
            return;
        }
        action(ts);
    }

    private string? TimeOfDayUTC
    {
        get => GetTimeSpanValue(Schedule.TimeOfDayUTC);
        set => SetTimeSpanValue(value, ts => Schedule.TimeOfDayUTC = ts);
    }

    private string? RecurTime
    {
        get => GetTimeSpanValue(Schedule.RecurTime);
        set => SetTimeSpanValue(value, ts => Schedule.RecurTime = ts);
    }

    private string? RecurBetweenStartUTC
    {
        get => GetTimeSpanValue(Schedule.RecurBetweenStartUTC);
        set => SetTimeSpanValue(value, ts => Schedule.RecurBetweenStartUTC = ts);
    }

    private string? RecurBetweenEndUTC
    {
        get => GetTimeSpanValue(Schedule.RecurBetweenEndUTC);
        set => SetTimeSpanValue(value, ts => Schedule.RecurBetweenEndUTC = ts);
    }

    [Parameter]
    [EditorRequired]
    public Schedule Schedule { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        ScheduleEditContext = new(Schedule);
        ScheduleEditContext.OnFieldChanged += (sender, e) =>
        {
            Console.WriteLine(TimeType);
            StateHasChanged();
        };

        await Task.CompletedTask;
    }
}
