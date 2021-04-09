using System;

namespace SimpleSchedulerModels
{
    public record SimpleTimeSpan(int Hours, int Minutes)
    {
        public TimeSpan AsTimeSpan() => TimeSpan.FromHours(Hours).Add(TimeSpan.FromMinutes(Minutes));
    }
}