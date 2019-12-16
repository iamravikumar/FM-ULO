using Newtonsoft.Json;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.ApplicationParts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Hangfire;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class RecurrenceSettings : IValidate
    {
        public class Occurrence
        {
            [DisplayName("Start Time (UTC)")]
            public DateTime StartAtUtc { get; }
            public TimeSpan Duration { get; }
            public DateTime EndsAtUtc { get; }
            public uint InstanceNumber { get; }

            public override string ToString() => $"{GetType().Name} startAt={StartAtUtc.ToLocalTime()}";

            internal Occurrence(DateTime startAtUtc, TimeSpan? duration, uint instanceNumber)
            {
                StartAtUtc = startAtUtc;
                Duration = duration == null ? TimeSpan.FromDays(1) : duration.Value;
                EndsAtUtc = StartAtUtc.Add(Duration);
                InstanceNumber = instanceNumber;
            }
        }

        [JsonIgnore]
        public Occurrence NextOccurrence
        {
            get
            {
                return GetOccurrences(1).FirstOrDefault(o => o.StartAtUtc >= DateTime.UtcNow);
            }
        }

        public IEnumerable<Occurrence> GetOccurrences(int maxPostNowOccurrences = 100)
        {
            Requires.Valid(this, this.GetType().Name);
            uint occNum = 0;
            var now = DateTime.UtcNow;
            int postNowOccurrences = 0;
            var dt = AllDay ?
                new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, 0, 0, 0, 0, DateTimeKind.Utc) :
                new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, StartTimeUtc.Hour, StartTimeUtc.Minute, StartTimeUtc.Second, StartTimeUtc.Millisecond, DateTimeKind.Utc);
            while (
                IsPerpetual ||
                (EndAfterNOccurrences > 0 && occNum < EndAfterNOccurrences) ||
                (EndBeforeDate.HasValue && dt < EndBeforeDate))
            {
                if (dt > now && ++postNowOccurrences > maxPostNowOccurrences) break;
                yield return new Occurrence(dt, Duration, occNum++);
                switch (PatternType)
                {
                    case PatternTypes.Daily:
                        if (DailyPattern.EveryNDays > 0)
                        {
                            dt = dt.AddDays(DailyPattern.EveryNDays);
                        }
                        else if (DailyPattern.EveryWeekday)
                        {
                            do
                            {
                                dt = dt.AddDays(1);
                            } while (!dt.IsWeekday());
                        }
                        else throw new NotImplementedException();
                        break;
                    case PatternTypes.Weekly:
                        for (; ; )
                        {
                            dt = dt.AddDays(1);
                            if (dt.DayOfWeek == DayOfWeek.Sunday)
                            {
                                dt = dt.AddDays((WeeklyPattern.RecurEveryNWeeksOn - 1) * 7);
                            }
                            if (WeeklyPattern.OccursOn(dt.DayOfWeek)) break;
                        }
                        break;
                }
            }
        }

        public enum PatternTypes
        {
            Hourly,
            Daily,
            Weekly,
            Monthly,
            Yearly,
        }

        public class HourlyPatternSettings : IValidate
        {
            [JsonProperty("everyNHours")]
            [DisplayName("Every")]
            public uint EveryNHours { get; set; }

            void IValidate.Validate()
            {
                if (EveryNHours > 24) throw new ArgumentOutOfRangeException($"{nameof(EveryNHours)} must be 24 or less");
            }
        }

        public class DailyPatternSettings : IValidate
        {

            [JsonProperty("everyNDays")]
            [DisplayName("Every")]
            public uint EveryNDays { get; set; }


            [JsonProperty("everyWeekday")]
            [DisplayName("Every weekday")]
            public bool EveryWeekday { get; set; } = true;

            public void Validate()
            {
                if (EveryNDays < 1 && !EveryWeekday) throw new ArgumentOutOfRangeException($"Exactly 1 of {nameof(EveryNDays)} and {nameof(EveryWeekday)} must be set");
                if (EveryNDays > 0 && EveryWeekday) throw new ArgumentOutOfRangeException($"Exactly 1 of {nameof(EveryNDays)} and {nameof(EveryWeekday)} must be set");
            }
        }

        public class WeeklyPatternSettings : IValidate
        {
            [Range(1, uint.MaxValue)]
            [JsonProperty("recurEveryNWeeksOn")]
            public uint RecurEveryNWeeksOn { get; set; } = 1;


            [JsonProperty("monday")]
            public bool Monday { get; set; } = true;


            [JsonProperty("tuesday")]
            public bool Tuesday { get; set; } = true;


            [JsonProperty("wednesday")]
            public bool Wednesday { get; set; } = true;


            [JsonProperty("thursday")]
            public bool Thursday { get; set; } = true;


            [JsonProperty("friday")]
            public bool Friday { get; set; } = true;


            [JsonProperty("saturday")]
            public bool Saturday { get; set; }


            [JsonProperty("sunday")]
            public bool Sunday { get; set; }

            public bool OccursOn(DayOfWeek dow)
            {
                switch (dow)
                {
                    case DayOfWeek.Sunday:
                        return Sunday;
                    case DayOfWeek.Monday:
                        return Monday;
                    case DayOfWeek.Tuesday:
                        return Tuesday;
                    case DayOfWeek.Wednesday:
                        return Wednesday;
                    case DayOfWeek.Thursday:
                        return Thursday;
                    case DayOfWeek.Friday:
                        return Friday;
                    case DayOfWeek.Saturday:
                        return Saturday;
                    default:
                        throw new UnexpectedSwitchValueException(dow);
                }
            }

            public void Validate()
            {
                Requires.Positive(RecurEveryNWeeksOn, nameof(RecurEveryNWeeksOn));
                Requires.True(Sunday || Monday || Tuesday || Wednesday || Thursday || Friday || Saturday, "At least one day must be specified");
            }
        }

#if false
        public class MonthlyPatternSettings
        {
            public bool FirstDayOfMonth { get; set; }
            public bool FirstWeekdayOfMonth { get; set; }
            public bool LastDayOfMonth { get; set; }
            public bool LastWeekdayOfMonth { get; set; }
            public uint DayOfDayDOfEveryNMonths { get; set; }
            public uint MonthsOfDayDOfEveryNMonths { get; set; }
            public NthWeeks NthWeek { get; set; }
            public DayOfWeek Day { get; set; }
            public uint OfEveryNMonths { get; set; }

        }

        public enum NthWeeks
        {
            First,
            Second,
            Third,
            Fourth,
            Last
        }

        public class YearlyPatternSettings
        { }
#endif


        [JsonProperty("startTimeUtc")]
        public DateTime StartTimeUtc { get; set; }


        [JsonProperty("allDay")]
        public bool AllDay { get; set; }


        [JsonProperty("duration")]
        public TimeSpan? Duration { get; set; }


        [JsonProperty("patternType")]
        public PatternTypes PatternType { get; set; }

        [JsonProperty("hourlyPattern")]
        public HourlyPatternSettings HourlyPattern { get; set; }

        [JsonProperty("dailyPattern")]
        public DailyPatternSettings DailyPattern { get; set; }


        [JsonProperty("weeklyPattern")]
        public WeeklyPatternSettings WeeklyPattern { get; set; }
        //public MonthlyPatternSettings MonthlyPattern { get; set; }
        //public YearlyPatternSettings YearlyPattern { get; set; }


        [JsonProperty("startDate")]
        [DisplayName("Start")]
        public DateTime StartDate { get; set; }


        [Range(1, uint.MaxValue)]
        [JsonProperty("endAfterNOccurrences")]
        [DisplayName("End after N occurrences")]
        public uint EndAfterNOccurrences { get; set; } = 1;


        [JsonProperty("endBeforeDate")]
        [DisplayName("End by date")]
        public DateTime? EndBeforeDate { get; set; }

        [JsonIgnore]
        public bool IsPerpetual => EndAfterNOccurrences == 0 && EndBeforeDate == null;

        public RecurrenceSettings()
        {
            var now = DateTime.UtcNow;
            StartDate = now.Date;
            StartTimeUtc = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc).AddHours(1);
            PatternType = PatternTypes.Hourly;
            DailyPattern = DailyPattern ?? new DailyPatternSettings();
            WeeklyPattern = WeeklyPattern ?? new WeeklyPatternSettings();
            HourlyPattern = HourlyPattern ?? new HourlyPatternSettings();
        }

        public RecurrenceSettings(string cronString)
        {
            var now = DateTime.UtcNow;
            DailyPattern = DailyPattern ?? new DailyPatternSettings
            {
                EveryWeekday = false
            };
            WeeklyPattern = WeeklyPattern ?? new WeeklyPatternSettings
            {
                Monday = false,
                Tuesday = false,
                Wednesday = false,
                Thursday = false,
                Friday = false,
                Saturday = false,
                Sunday = false,
            };
            HourlyPattern = HourlyPattern ?? new HourlyPatternSettings();

            var values = cronString.Split(' ').ToList();

            if (values.Count() == 5)
            {
                cronString = "* " + cronString + " *";
                values = cronString.Split(' ').ToList();
            }

            if (values.Count() == 6)
            {
                values.Add("*"); //explicitely declare every year
            }
            if (values.Count() != 7)
            {
                throw new Exception("Invalid cron string");
            }

            var startMinute = 0;
            var startHour = 0;
            if (values[1] != "*")
            {
                startMinute = int.Parse(values[1]);
            }

            if (!values[2].Contains("*"))
            {
                startHour = int.Parse(values[2]);
            }

            StartDate = now.Date;
            StartTimeUtc = new DateTime(now.Year, now.Month, now.Day, startHour, startMinute, 0, DateTimeKind.Utc);

            if (values[4] != "*")
            {
                //Expression is yearly
                throw new NotImplementedException();
            }
            else if (values[2].Contains(@"*/") && values[3] == "*" && values[4] == "*" && values[5] == "*" && values[6] == "*")
            {
                //Expression is hourly
                PatternType = PatternTypes.Hourly;

                var hourInterval = values[2].Replace(@"*/", "");
                HourlyPattern.EveryNHours = uint.Parse(hourInterval);
            }
            else if (!values[2].Contains(@"*/") && values[3] == "*" && values[5] == "*")
            {
                //Expression is daily - every day
                PatternType = PatternTypes.Daily;

                DailyPattern.EveryNDays = 1;
            }
            else if (values[5] == "1-5" || values[5] == "1,2,3,4,5")
            {
                //Expression is daily - weekdays
                PatternType = PatternTypes.Daily;

                DailyPattern.EveryWeekday = true;
            }
            else if (values[5].IndexOf("#") == -1 && values[5].IndexOf('L') == -1 && values[5] != "?")
            {
                //Expression is weekly
                PatternType = PatternTypes.Weekly;

                if (values[5].IndexOf("-") > 0)
                {
                    var inDays = values[5].Split('-');
                    for (var i = int.Parse(inDays[0]); i <= int.Parse(inDays[1]); i++)
                    {
                        if (i == 0)
                        {
                            WeeklyPattern.Sunday = true;
                        }
                        if (i == 1)
                        {
                            WeeklyPattern.Monday = true;
                        }
                        if (i == 2)
                        {
                            WeeklyPattern.Tuesday = true;
                        }
                        if (i == 3)
                        {
                            WeeklyPattern.Wednesday = true;
                        }
                        if (i == 4)
                        {
                            WeeklyPattern.Thursday = true;
                        }
                        if (i == 5)
                        {
                            WeeklyPattern.Friday = true;
                        }
                        if (i == 6)
                        {
                            WeeklyPattern.Saturday = true;
                        }
                    }
                }
                else
                {
                    var commaSeparatedDays = values[5].Split(',');
                    foreach (var day in commaSeparatedDays)
                    {
                        if (int.Parse(day) == 0)
                        {
                            WeeklyPattern.Sunday = true;
                        }
                        if (int.Parse(day) == 1)
                        {
                            WeeklyPattern.Monday = true;
                        }
                        if (int.Parse(day) == 2)
                        {
                            WeeklyPattern.Tuesday = true;
                        }
                        if (int.Parse(day) == 3)
                        {
                            WeeklyPattern.Wednesday = true;
                        }
                        if (int.Parse(day) == 4)
                        {
                            WeeklyPattern.Thursday = true;
                        }
                        if (int.Parse(day) == 5)
                        {
                            WeeklyPattern.Friday = true;
                        }
                        if (int.Parse(day) == 6)
                        {
                            WeeklyPattern.Saturday = true;
                        }
                    }
                }
            }
            else
            {
                //Expression is something we don't support yet
                throw new NotImplementedException();
            }

        }

        public void Validate()
        {
            switch (PatternType)
            {
                case PatternTypes.Hourly:
                    Requires.NonNull(HourlyPattern, nameof(HourlyPattern));
                    Requires.Valid(HourlyPattern, nameof(HourlyPattern));
                    break;
                case PatternTypes.Daily:
                    Requires.NonNull(DailyPattern, nameof(DailyPattern));
                    Requires.Valid(DailyPattern, nameof(DailyPattern));
                    break;
                case PatternTypes.Weekly:
                    Requires.NonNull(WeeklyPattern, nameof(WeeklyPattern));
                    Requires.Valid(WeeklyPattern, nameof(WeeklyPattern));
                    break;
                case PatternTypes.Monthly:
                    throw new NotImplementedException();
                case PatternTypes.Yearly:
                    throw new NotImplementedException();
                default:
                    throw new UnexpectedSwitchValueException(PatternType);
            }
        }

        public string ConvertToCronString()
        {
            switch (PatternType)
            {
                case PatternTypes.Hourly:
                    Requires.NonNull(HourlyPattern, nameof(HourlyPattern));
                    Requires.Valid(HourlyPattern, nameof(HourlyPattern));
                    return Cron.HourInterval(Convert.ToInt32(HourlyPattern.EveryNHours));
                case PatternTypes.Daily:
                    Requires.NonNull(DailyPattern, nameof(DailyPattern));
                    Requires.Valid(DailyPattern, nameof(DailyPattern));
                    if (DailyPattern.EveryNDays > 0)
                    {
                        return Cron.DayInterval(Convert.ToInt32(DailyPattern.EveryNDays));
                    }
                    if (DailyPattern.EveryWeekday)
                    {
                        var startTimeHour = StartTimeUtc.Hour.ToString();
                        var startTimeMinute = StartTimeUtc.Minute.ToString();
                        return $"{startTimeMinute} {startTimeHour} * * 1-5";
                    }
                    break;
                case PatternTypes.Weekly:
                    Requires.NonNull(WeeklyPattern, nameof(WeeklyPattern));
                    Requires.Valid(WeeklyPattern, nameof(WeeklyPattern));

                    if (WeeklyPattern.RecurEveryNWeeksOn == 1)
                    {
                        var days = new List<int>();
                        //TODO: Test trailing comma
                        if (WeeklyPattern.Monday)
                        {
                            days.Add(1);
                        }
                        if (WeeklyPattern.Tuesday)
                        {
                            days.Add(2);
                        }
                        if (WeeklyPattern.Wednesday)
                        {
                            days.Add(3);
                        }
                        if (WeeklyPattern.Thursday)
                        {
                            days.Add(4);
                        }
                        if (WeeklyPattern.Friday)
                        {
                            days.Add(5);
                        }
                        if (WeeklyPattern.Saturday)
                        {
                            days.Add(6);
                        }
                        if (WeeklyPattern.Sunday)
                        {
                            days.Add(7);
                        }
                        var dayString = String.Join(",", days);
                        var startTimeHour = StartTimeUtc.Hour.ToString();
                        var startTimeMinute = StartTimeUtc.Minute.ToString();

                        return $"{startTimeMinute} {startTimeHour} * * {dayString}";
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                case PatternTypes.Monthly:
                    throw new NotImplementedException();
                case PatternTypes.Yearly:
                    throw new NotImplementedException();
                default:
                    throw new UnexpectedSwitchValueException(PatternType);
            }

            return "";
        }
    }
}
