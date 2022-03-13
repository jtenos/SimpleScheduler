using System.Collections.Immutable;

namespace SimpleSchedulerService;

public static class ScheduleFinder
{
    public static DateTime GetNextDate(DateTime? lastQueueDate, ImmutableArray<DayOfWeek> daysOfTheWeek, TimeSpan? timeOfDay,
        TimeSpan? recurrence, TimeSpan? recurBetweenStart, TimeSpan? recurBetweenEnd)
    {
        if (daysOfTheWeek == null || !daysOfTheWeek.Any())
        {
            throw new ApplicationException("No days of the week selected");
        }

        return timeOfDay != null
            ? ProcessFixedTime(lastQueueDate, daysOfTheWeek, timeOfDay.Value)
            : ProcessRecurringTime(lastQueueDate, daysOfTheWeek,
            recurrence!.Value, recurBetweenStart, recurBetweenEnd);
    }

    private static DateTime ProcessFixedTime(DateTime? lastQueueDate, ImmutableArray<DayOfWeek> daysOfTheWeek, TimeSpan timeOfDay)
    {
        lastQueueDate ??= DateTime.UtcNow;

        if (
            CheckDay(lastQueueDate, daysOfTheWeek)
            && (
                lastQueueDate.Value.Hour < timeOfDay.Hours
                || lastQueueDate.Value.Hour == timeOfDay.Hours && lastQueueDate.Value.Minute < timeOfDay.Minutes
                || lastQueueDate.Value.Hour == timeOfDay.Hours && lastQueueDate.Value.Minute == timeOfDay.Minutes && lastQueueDate.Value.Second < timeOfDay.Seconds
            )
        )
        {
            return lastQueueDate.Value.Date.AddHours(timeOfDay.Hours).AddMinutes(timeOfDay.Minutes).AddSeconds(timeOfDay.Seconds);
        }

        // today didn't work, so pick the next valid date
        var theDate = lastQueueDate.Value.Date.AddDays(1);
        while (true)
        {
            if (CheckDay(theDate, daysOfTheWeek))
            {
                return theDate.AddHours(timeOfDay.Hours).AddMinutes(timeOfDay.Minutes).AddSeconds(timeOfDay.Seconds);
            }

            theDate = theDate.AddDays(1);
        }
    }

    private static DateTime ProcessRecurringTime(DateTime? lastQueueDate,
        ImmutableArray<DayOfWeek> daysOfTheWeek, TimeSpan recurrence, TimeSpan? recurBetweenStart, TimeSpan? recurBetweenEnd)
    {
        if (recurBetweenStart == null)
        {
            recurBetweenStart = TimeSpan.Zero;
        }
        if (recurBetweenEnd == null)
        {
            recurBetweenEnd = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)).Add(TimeSpan.FromSeconds(59));
        }

        DateTime queueDate;

        if (!lastQueueDate.HasValue)
        {
            queueDate = DateTime.UtcNow;
        }
        else
        {
            queueDate = lastQueueDate.Value.Add(recurrence);
        }

        if (
            CheckDay(queueDate, daysOfTheWeek)
            && queueDate.TimeOfDay <= recurBetweenEnd
        )
        {
            if (queueDate.TimeOfDay >= recurBetweenStart)
            {
                // Nothing
            }
            else if (queueDate.TimeOfDay < recurBetweenStart)
            {
                queueDate = queueDate.Date.Add(recurBetweenStart.Value);
            }
        }
        else
        {
            queueDate = queueDate.Date.AddDays(1);
            while (true)
            {
                if (CheckDay(queueDate, daysOfTheWeek))
                {
                    return queueDate.Add(recurBetweenStart.Value);
                }
                queueDate = queueDate.AddDays(1);
            }
        }

        return queueDate;
    }

    private static bool CheckDay(DateTime? lastQueueDate, ImmutableArray<DayOfWeek> daysOfTheWeek)
    {
        if (!lastQueueDate.HasValue) return false;

        return daysOfTheWeek.Contains(lastQueueDate.Value.DayOfWeek);
    }
}
