using System;
using System.Globalization;
#if STATICS
using Supercent.Statics;
#endif

namespace Supercent.Util
{
    public static class TimeUtil
    {
        private static readonly LogClassPrinter _printer = new LogClassPrinter(nameof(TimeUtil), $"#FF0000");
        public const string DEFAULT_DATE_FORMAT = "yyyy/MM/dd/HH/mm";


        public static DateTime ParseDateTime(this string dateTimeText, string dateFormat = DEFAULT_DATE_FORMAT)
        {
            if (!DateTime.TryParseExact(dateTimeText, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
            {
                dateTime = DateTime.MinValue;
                _printer.Error("ParseDateTime", $"{dateTimeText}를 파싱할 수 없습니다.");
            }

            return dateTime;
        }

        /// <summary>
        /// 남은 시간의 크기와 상관 없이 분:초 00:00 로 표기
        /// </summary>
        public static string GetRemainTimeText_mmss(this TimeSpan inTimeSpan)
        {
            var totalSec = inTimeSpan.TotalSeconds;

            if (totalSec <= 0)
                return GetEndedText();

            var mm = (int)totalSec / 60;
            var ss = (int)totalSec % 60;

            return $"{mm:00}:{ss:00}";
        }

        private static string GetEndedText()
        {
#if STATICS
            var localeKey = "ENDED";
            var localeText = localeKey.Locale();

            // 로케일이 없다면 영문 Ended로 대체한다.
            if (localeKey == localeText)
                return "Ended";

            return "ENDED".Locale();
#else
            return "Ended";
#endif
        }

        /// <summary>
        /// 남은 시간 표기의 큰 단위 2개로만 표기 (ex. dd일 hh시간, hh 시간 mm분, mm분 ss초)
        /// </summary>
        public static string GetRemainTimeText(this TimeSpan inTimeSpan)
        {
            string timeStr;
            int dd = (int)inTimeSpan.TotalDays;
            if (dd > 0)
            {
                int hh = (int)inTimeSpan.Hours;
                timeStr = $"{dd}d {hh}h";
            }
            else
            {
                int hh = (int)inTimeSpan.Hours;
                if (hh > 0)
                {
                    int mm = (int)inTimeSpan.Minutes;
                    timeStr = $"{hh}h {mm}m";
                }
                else
                {
                    return GetRemainTimeText_mmss(inTimeSpan);
                }
            }

            return timeStr;
        }

    }
}