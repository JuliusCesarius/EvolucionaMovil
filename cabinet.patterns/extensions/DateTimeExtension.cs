using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cabinet.patterns.clases;

namespace cabinet.patterns.extensions
{
    public static class DateTimeExtension
    {
        public static string ToString(this DateTime dateTime, string format = null, IFormatProvider formatProvider = null)
        {
            TimeZoneInfo timeZone = DateTimeUtil.CurrentTimeZone;
            return DateTimeUtil.ToString(dateTime, timeZone, format, formatProvider);
        }

        public static string ToString(this DateTime? dateTime, string format = null, IFormatProvider formatProvider = null)
        {
            TimeZoneInfo timeZone = DateTimeUtil.CurrentTimeZone;
            return DateTimeUtil.ToString((DateTime)dateTime, timeZone, format, formatProvider);
        }
        public static DateTime GetCurrentTime(this DateTime dateTime)
        {
            TimeZoneInfo timeZone = DateTimeUtil.CurrentTimeZone;
            return TimeZoneInfo.ConvertTime(dateTime, timeZone);            
        }

    }
}