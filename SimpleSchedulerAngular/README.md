# Sch

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 11.0.2.

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`. The app will automatically reload if you change any of the source files.

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory. Use the `--prod` flag for a production build.

## Running unit tests

Run `ng test` to execute the unit tests via [Karma](https://karma-runner.github.io).

## Running end-to-end tests

Run `ng e2e` to execute the end-to-end tests via [Protractor](http://www.protractortest.org/).

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI Overview and Command Reference](https://angular.io/cli) page.




TODO:


    {
        public ImmutableArray<DayOfWeek> GetDaysOfWeek()
        {
            var result = new List<DayOfWeek>();
            if (Sunday) { result.Add(DayOfWeek.Sunday); }
            if (Monday) { result.Add(DayOfWeek.Monday); }
            if (Tuesday) { result.Add(DayOfWeek.Tuesday); }
            if (Wednesday) { result.Add(DayOfWeek.Wednesday); }
            if (Thursday) { result.Add(DayOfWeek.Thursday); }
            if (Friday) { result.Add(DayOfWeek.Friday); }
            if (Saturday) { result.Add(DayOfWeek.Saturday); }

            return result.ToImmutableArray();
        }

        public string GetDaysFormatted()
        {
            if (Sunday && Monday && Tuesday && Wednesday && Thursday && Friday && Saturday) { return "Every day"; }
            if (!Sunday && Monday && Tuesday && Wednesday && Thursday && Friday && !Saturday) { return "Weekdays"; }
            if (Sunday && !Monday && !Tuesday && !Wednesday && !Thursday && !Friday && Saturday) { return "Weekends"; }

            var days = new List<string>();
            if (Sunday) { days.Add("Sunday"); }
            if (Monday) { days.Add("Monday"); }
            if (Tuesday) { days.Add("Tuesday"); }
            if (Wednesday) { days.Add("Wednesday"); }
            if (Thursday) { days.Add("Thursday"); }
            if (Friday) { days.Add("Friday"); }
            if (Saturday) { days.Add("Saturday"); }

            return string.Join(", ", days);
        }

        public string GetTimeFormatted()
        {
            if (TimeOfDayUTC.HasValue)
            {
                return $"at {TimeOfDayUTC.Value:hh\\:mm} (UTC)";
            }
            if (RecurTime.HasValue)
            {
                string result;
                if (RecurTime.Value.Hours > 0 && RecurTime.Value.Minutes == 0)
                {
                    if (RecurTime.Value.Hours == 1)
                    {
                        result = "every hour";
                    }
                    else
                    {
                        result = $"every {RecurTime.Value.Hours} hours";
                    }
                }
                else if (RecurTime.Value.Hours == 0 && RecurTime.Value.Minutes > 0)
                {
                    if (RecurTime.Value.Minutes == 1)
                    {
                        result = "every minute";
                    }
                    else
                    {
                        result = $"every {RecurTime.Value.Minutes} minutes";
                    }
                }
                else
                {
                    result = $"every {RecurTime:hh\\:mm}";
                }

                if (RecurBetweenStartUTC.HasValue && RecurBetweenEndUTC.HasValue)
                {
                    return $"{result} between {RecurBetweenStartUTC:hh\\:mm} and {RecurBetweenEndUTC:hh\\:mm}";
                }
                if (RecurBetweenStartUTC.HasValue)
                {
                    return $"{result} on and after {RecurBetweenStartUTC:hh\\:mm}";
                }
                if (RecurBetweenEndUTC.HasValue)
                {
                    return $"{result} before {RecurBetweenEndUTC:hh\\:mm}";
                }
                return result;
            }
            return "";
        }