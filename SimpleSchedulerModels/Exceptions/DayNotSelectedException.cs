using System;

namespace SimpleSchedulerModels.Exceptions
{
    public class DayNotSelectedException
        : ApplicationException
    {
        public DayNotSelectedException() : base("You must select a day") { }
    }
}
