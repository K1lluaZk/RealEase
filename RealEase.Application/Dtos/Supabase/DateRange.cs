using System;
using System.Globalization;

namespace RealEase.Application.Dtos.Supabase
{
    /// <summary>
    /// Encapsulates a date range [Start, End) matching PostgreSQL's daterange type.
    /// </summary>
    public struct DateRange
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public DateRange(DateTime start, DateTime end)
        {
            if (end < start)
                throw new ArgumentException("End date cannot be earlier than start date.");

            Start = start.Date;
            End = end.Date;
        }

        public int TotalNights => (int)(End - Start).TotalDays;

        public bool Contains(DateTime date)
        {
            var d = date.Date;
            return d >= Start && d < End;
        }

        public bool ContainsRange(DateRange other)
        {
            return Start <= other.Start && End >= other.End;
        }

        public bool Overlaps(DateRange other)
        {
            return Start < other.End && other.Start < End;
        }

        /// <summary>
        /// Converts to PostgreSQL daterange literal: [YYYY-MM-DD,YYYY-MM-DD)
        /// </summary>
        public override string ToString()
        {
            return $"[{Start:yyyy-MM-dd},{End:yyyy-MM-dd})";
        }

        /// <summary>
        /// Parses a PostgreSQL daterange string such as "[2026-09-01,2026-09-10)"
        /// </summary>
        public static DateRange Parse(string daterangeString)
        {
            if (string.IsNullOrWhiteSpace(daterangeString))
                throw new ArgumentNullException(nameof(daterangeString));

            var trimmed = daterangeString.Trim('[', ']', '(', ')', ' ');
            var parts = trimmed.Split(',');
            if (parts.Length != 2)
                throw new FormatException($"Invalid daterange format: {daterangeString}");

            var start = DateTime.ParseExact(parts[0].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var end = DateTime.ParseExact(parts[1].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);

            return new DateRange(start, end);
        }

        public static bool TryParse(string daterangeString, out DateRange range)
        {
            try
            {
                range = Parse(daterangeString);
                return true;
            }
            catch
            {
                range = default;
                return false;
            }
        }
    }
}
