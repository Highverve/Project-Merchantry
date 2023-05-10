using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.Culture
{
    public class Calendar
    {
        #region Seasons

        public Dictionary<int, string> Seasons { get; private set; } = new Dictionary<int, string>();
        public void SetDefaultSeasons()
        {
            Seasons.Add(1, "Spring");
            Seasons.Add(2, "Summer");
            Seasons.Add(3, "Autumn");
            Seasons.Add(4, "Winter");
        }
        public string Season() { return Seasons[Quarter]; }

        #endregion

        #region Counters

        public int Year { get; set; }
        public int Quarter { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        /// <summary>
        /// Realtime used to progress minutes.
        /// </summary>
        public float Milliseconds { get; set; }

        #endregion

        #region Time Per

        public int MillisecondsPerMinute { get; set; } = 100;
        public int MinutesPerHour { get; set; } = 60;
        public int HoursPerDay { get; set; } = 23;
        public int DaysPerQuarter { get; set; } = 30;
        public int QuartersPerYear { get; set; } = 4;

        #endregion

        #region Percentages

        public float MinutePercentage() { return Milliseconds / MillisecondsPerMinute; }
        public float HourPercentage() { return ((Minute - 1) + MinutePercentage()) / MinutesPerHour; }
        public float DayPercentage() { return ((Hour - 1) + HourPercentage()) / HoursPerDay; }
        public float QuarterPercentage() { return ((Day - 1) + DayPercentage()) / DaysPerQuarter; }
        public float YearPercentage() { return ((Quarter - 1) + QuarterPercentage()) / QuartersPerYear; }

        #endregion

        private DateStamp now;
        public DateStamp Now() { return now; }
        private void UpdateNow() { now = new DateStamp(Year, Quarter, Day, Hour, Minute); }

        public Calendar(int Year, int Quarter, int Day, int Hour, int Minute)
        {
            this.Year = Year;
            this.Quarter = Quarter;
            this.Day = Day;
            this.Hour = Hour;
            this.Minute = Minute;

            SetDefaultSeasons();
            UpdateNow();
            onMinuteChange += (i) => UpdateNow();
        }

        public void Validate()
        {
            //Moving forward
            if (Minute >= MinutesPerHour) { Hour++; onHourChange?.Invoke(Hour); Minute = 0; }
            if (Hour > HoursPerDay) { Day++; onDayChange?.Invoke(Day); Hour = 0; }
            if (Day > DaysPerQuarter) { Quarter++; onSeasonChange?.Invoke(Quarter); Day = 1; }
            if (Quarter > QuartersPerYear) { Year++; onYearChange?.Invoke(Year); Quarter = 1; }

            //Moving backward...?
            if (Minute < 0) { Hour--; onHourChange?.Invoke(Hour); Minute = 59; }
            if (Hour < 0) { Day--; onDayChange?.Invoke(Day); Hour = 23; }
            if (Day < 1) { Quarter--; onSeasonChange?.Invoke(Quarter); Day = 30; }
            if (Quarter < 1) { Year--; onYearChange?.Invoke(Year); Quarter = 1; }
        }
        public void Update(GameTime gt)
        {
            if (GameManager.References().Settings.IsPaused == false)
            {
                Milliseconds += (float)gt.ElapsedGameTime.TotalMilliseconds;
                if (Milliseconds >= MillisecondsPerMinute)
                {
                    Minute++;
                    onMinuteChange?.Invoke(Minute);
                    Milliseconds -= MillisecondsPerMinute;

                    Validate();
                }
            }
        }

        #region Events

        private event Action<int> onMinuteChange, onHourChange, onDayChange, onSeasonChange, onYearChange;
        public event Action<int> OnMinuteChange { add { onMinuteChange += value; } remove { onMinuteChange -= value; } }
        public event Action<int> OnHourChange { add { onHourChange += value; } remove { onHourChange -= value; } }
        public event Action<int> OnDayChange { add { onDayChange += value; } remove { onDayChange -= value; } }
        public event Action<int> OnSeasonChange { add { onSeasonChange += value; } remove { onSeasonChange -= value; } }
        public event Action<int> OnYearChange { add { onYearChange += value; } remove { onYearChange -= value; } }

        public void OnTime(int year, int quarter, int day, int hour, int minute, Action action)
        {
            onMinuteChange += (i) =>
            {
                if (Year == year && Quarter == quarter &&
                    Day == day && Hour == hour && Minute == minute)
                {
                    action?.Invoke();
                }
            };
        }
        public void OnTime(int year, int quarter, int day, int hour, Action action)
        {
            onHourChange += (i) =>
            {
                if (Year == year && Quarter == quarter &&
                    Day == day && Hour == hour)
                {
                    action?.Invoke();
                }
            };
        }
        public void OnTime(int year, int quarter, int day, Action action)
        {
            onDayChange += (i) =>
            {
                if (Year == year && Quarter == quarter && Day == day)
                    action?.Invoke();
            };
        }
        public void OnTime(int year, int quarter, Action action)
        {
            onSeasonChange += (i) =>
            {
                if (Year == year && Quarter == quarter)
                    action?.Invoke();
            };
        }

        #endregion
    }

    public struct DateStamp
    {
        public int Year { get; set; }
        public int Quarter { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }

        public DateStamp(int Year, int Quarter, int Day, int Hour, int Minute)
        {
            this.Year = Year;
            this.Quarter = Quarter;
            this.Day = Day;
            this.Hour = Hour;
            this.Minute = Minute;
        }

        public override string ToString()
        {
            return Year + "-" + Quarter + "-" + Day + " " + Hour + ":" + Minute;
        }

        /*public static DateStamp operator + (DateStamp a, DateStamp b)
        {
            return new DateStamp(a.Year + b.Year,
                                 a.Quarter + b.Quarter,
                                 a.Day + b.Day,
                                 a.Hour + b.Hour,
                                 a.Minute + b.Minute);
        }
        public static DateStamp operator - (DateStamp a, DateStamp b)
        {
            return new DateStamp(a.Year - b.Year,
                                 a.Quarter - b.Quarter,
                                 a.Day - b.Day,
                                 a.Hour - b.Hour,
                                 a.Minute - b.Minute);
        }*/
    }
}
