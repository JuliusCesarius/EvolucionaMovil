using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Globalization;

namespace EvolucionaMovil.Models.Classes
{
    public static class DateTimeUtil
    {
        #region TimeZone Retrieval
        /// <summary>
        /// Retrieves the timezone defined in the appSettings section with the key "TimeZone"
        /// </summary>
        public static TimeZoneInfo CurrentTimeZone
        {
            get
            {
                string timeZone;
                if (string.IsNullOrWhiteSpace(timeZone = ConfigurationManager.AppSettings["TimeZone"]))
                {
                    return TimeZoneInfo.Local;
                }
                return TimeZoneInfo.FindSystemTimeZoneById(timeZone.Trim());
            }
        }
        #endregion
        public static string ToCurrentTimeZoneString( DateTime dateTime, string format = null, IFormatProvider formatProvider = null)
        {
            TimeZoneInfo timeZone = CurrentTimeZone;
            return ToString(dateTime, timeZone, format, formatProvider);
        }
        public static string ToString(this DateTime dateTime, string format = null, IFormatProvider formatProvider = null)
        {
            TimeZoneInfo timeZone = CurrentTimeZone;
            return ToString(dateTime, timeZone, format, formatProvider);
        }
        #region DateTime to String Conversion
        /// <summary>
        /// Formats a date/time object to the specified time zone
        /// </summary>
        /// <param name="dateTime">Date/Time to convert</param>
        /// <param name="timeZone">Destination time zone</param>
        /// <param name="format">Format</param>
        /// <param name="formatProvider">Formatting provider</param>
        /// <returns>Textual representation of the date/time in the specified timezone</returns>
        public static string ToString(this DateTime dateTime, TimeZoneInfo timeZone, string format = null, IFormatProvider formatProvider = null)
        {
            if (timeZone == null)
            {
                timeZone = TimeZoneInfo.Local;
            }
            if (formatProvider == null)
            {
                formatProvider = CultureInfo.CurrentCulture;
            }

            // Convert to the specified time zone
            dateTime = TimeZoneInfo.ConvertTime(dateTime, timeZone);

            // Format
            if (format == null)
            {
                return dateTime.ToString(formatProvider);
            }
            else
            {
                return dateTime.ToString(format, formatProvider);
            }
        }
        #endregion
    }
}