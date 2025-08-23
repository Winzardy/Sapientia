using System;
using Sapientia.Extensions;

namespace Sapientia
{
	/// <inheritdoc cref="ISchedulePoint.Kind"/>
	public ref struct SchedulePointCode
	{
		private long _code;

		public static implicit operator long(SchedulePointCode x) => x._code;
		public static implicit operator SchedulePointCode(long code) => new() {_code = code};
	}

	/// <inheritdoc cref="ISchedulePoint.Kind"/>
	public ref struct SchedulePointDecode
	{
		private const long DATE_OFFSET = 100000L;

		private const long MONTHLY_OFFSET = 100000L;

		private const long MONTH_YEARLY_OFFSET = 10000000L;
		private const long DAY_YEARLY_OFFSET = 100000L;
		private const long DAY_YEARLY_CAPACITY = 100L;

		private const long DAY_OF_WEEK_OFFSET = 100000L;
		private const int DAY_OF_WEEK_OFFSET_CAPACITY = 10;

		private const int WEEK_OF_MONTH_CAPACITY = 10;
		private const long WEEK_OF_MONTH_OFFSET = DAY_OF_WEEK_OFFSET * WEEK_OF_MONTH_CAPACITY;
		private const long MONTH_WEEKLY_OFFSET = WEEK_OF_MONTH_OFFSET * WEEK_OF_MONTH_CAPACITY;

		public SchedulePointKind kind;

		/// <summary>
		/// Знак где-то участвует, а где-то нет, но в основном он указывает от чего считать с начала или с конца
		/// </summary>
		public bool sign;

		public long yr;

		/// <summary>
		/// Начинается с 0 (например, 0 = январь)
		/// </summary>
		public byte mh;

		/// <summary>
		/// Начинается с 0 (например, 0 = первый день)
		/// </summary>
		public long day;

		public byte hr;
		public byte min;
		public long sec;

		/// <summary>
		/// Порядковый номер недели (с 0) в месяце (<c>0</c> = первая, <c>1</c> = вторая и т.д.)<br/>
		/// Если общий код отрицательный, то:
		/// <c>-1</c> = последняя, <c>-2</c> = предпоследняя и т.д.
		/// </summary>
		public byte weekOfMonth;

		public static implicit operator SchedulePointDecode(long rawCode) => Decode(rawCode);
		public static implicit operator SchedulePointDecode(SchedulePointCode code) => Decode(code);

		public SchedulePointDecode(SchedulePointKind kind) : this()
		{
			this.kind = kind;
		}

		public static SchedulePointDecode Decode(SchedulePointCode code)
		{
			long rawCode = code;

			var sign = rawCode >= 0;

			var absRawCode = sign ? rawCode : -rawCode;
			var kind = ScheduleUtility.GetKind(absRawCode);

			var offset = absRawCode / ISchedulePoint.TYPE_OFFSET;

			switch (kind)
			{
				case SchedulePointKind.Interval:
					return new SchedulePointDecode(kind) {sec = offset};

				case SchedulePointKind.Daily:
				{
					var (hr, min, sec) = Split(offset);
					return new SchedulePointDecode(kind) {hr = hr, min = min, sec = sec};
				}

				case SchedulePointKind.Monthly:
				{
					var day = offset / MONTHLY_OFFSET;
					var (hr, min, sec) = Split(offset % MONTHLY_OFFSET);

					return new SchedulePointDecode(kind) {day = day, hr = hr, min = min, sec = sec, sign = sign};
				}

				case SchedulePointKind.Yearly:
				{
					var mh = Math.Abs(offset / MONTH_YEARLY_OFFSET);
					var day = offset / DAY_YEARLY_OFFSET % DAY_YEARLY_CAPACITY;
					var (hr, min, sec) = Split(offset % DAY_YEARLY_OFFSET);
					return new SchedulePointDecode(kind) {mh = (byte) mh, day = day, hr = hr, min = min, sec = sec, sign = sign};
				}

				case SchedulePointKind.Weekly:
				{
					var day = offset / DAY_OF_WEEK_OFFSET;
					var (hr, min, sec) = Split(offset % DAY_OF_WEEK_OFFSET);
					return new SchedulePointDecode(kind) {day = day, hr = hr, min = min, sec = sec};
				}

				case SchedulePointKind.MonthlyOnWeekday:
				{
					var weekOfMonth = offset / WEEK_OF_MONTH_OFFSET;
					var day = offset / DAY_OF_WEEK_OFFSET % DAY_OF_WEEK_OFFSET_CAPACITY;
					var (hr, min, sec) = Split(offset % DAY_OF_WEEK_OFFSET);
					return new SchedulePointDecode(kind)
						{weekOfMonth = (byte) weekOfMonth, day = day, hr = hr, min = min, sec = sec, sign = sign};
				}

				case SchedulePointKind.YearlyOnWeekday:
				{
					var mh = offset / MONTH_WEEKLY_OFFSET;
					var weekOfMonth = offset / WEEK_OF_MONTH_OFFSET % WEEK_OF_MONTH_CAPACITY;
					var day = offset / DAY_OF_WEEK_OFFSET % DAY_OF_WEEK_OFFSET_CAPACITY;
					var (hr, min, sec) = Split(offset % DAY_OF_WEEK_OFFSET);
					return new SchedulePointDecode(kind)
						{mh = (byte) mh, weekOfMonth = (byte) weekOfMonth, day = day, hr = hr, min = min, sec = sec, sign = sign};
				}

				case SchedulePointKind.Date:
				{
					var daysSinceEpoch = offset / DATE_OFFSET;
					var seconds = offset % DATE_OFFSET;

					var date = DateTime.UnixEpoch.AddDays(sign ? daysSinceEpoch : -daysSinceEpoch);

					var (hr, min, sec) = Split(seconds);
					return new SchedulePointDecode(kind)
					{
						yr = date.Year,
						mh = (byte) (date.Month - 1),
						day = date.Day - 1,
						hr = hr,
						min = min,
						sec = sec,
						sign = sign
					};
				}

				default:
					throw new NotImplementedException($"Unknown SchedulePointType (int: {kind}, code:{code})");
			}

			(byte hr, byte min, byte sec) Split(long seconds)
			{
				var hr = seconds / TimeUtility.SECS_IN_ONE_HOUR % 24;
				var min = (seconds / TimeUtility.MINS_IN_ONE_HOUR)
					% TimeUtility.SECS_IN_ONE_MINUTE;
				var sec = seconds % TimeUtility.SECS_IN_ONE_MINUTE;
				return ((byte) hr, (byte) min, (byte) sec);
			}
		}

		public static SchedulePointCode Encode(in SchedulePointDecode decode)
		{
			var kindInt = decode.kind.ToInt();
			var sign = decode.sign ? 1 : -1;
			switch (decode.kind)
			{
				case SchedulePointKind.Interval:
				{
					return decode.sec * ISchedulePoint.TYPE_OFFSET + kindInt;
				}

				case SchedulePointKind.Daily:
				{
					return Combine(decode.hr, decode.min, decode.sec) * ISchedulePoint.TYPE_OFFSET + kindInt;
				}

				case SchedulePointKind.Monthly:
				{
					var code = (decode.day * MONTHLY_OFFSET +
							Combine(decode.hr, decode.min, decode.sec))
						* ISchedulePoint.TYPE_OFFSET + kindInt;
					return sign * code;
				}

				case SchedulePointKind.Yearly:
				{
					var code = (decode.mh * MONTH_YEARLY_OFFSET +
						decode.day * DAY_YEARLY_OFFSET +
						Combine(decode.hr, decode.min, decode.sec)) * ISchedulePoint.TYPE_OFFSET + kindInt;
					return sign * code;
				}

				case SchedulePointKind.Weekly:
				{
					return ToWeeklyOffset(in decode) * ISchedulePoint.TYPE_OFFSET + kindInt;
				}

				case SchedulePointKind.MonthlyOnWeekday:
				{
					var code = (decode.weekOfMonth * WEEK_OF_MONTH_OFFSET +
						ToWeeklyOffset(in decode)) * ISchedulePoint.TYPE_OFFSET + kindInt;
					return sign * code;
				}

				case SchedulePointKind.YearlyOnWeekday:
				{
					var code = (decode.mh * MONTH_WEEKLY_OFFSET +
						decode.weekOfMonth * WEEK_OF_MONTH_OFFSET +
						ToWeeklyOffset(in decode)) * ISchedulePoint.TYPE_OFFSET + kindInt;
					return sign * code;
				}

				case SchedulePointKind.Date:
				{
					var date = new DateTime(
						(int) decode.yr,
						decode.mh + 1,
						(int) decode.day + 1, 0, 0, 0, DateTimeKind.Utc);
					var day = (date - DateTime.UnixEpoch).Days.Abs();

					var code = (day * DATE_OFFSET +
							Combine(decode.hr, decode.min, decode.sec))
						* ISchedulePoint.TYPE_OFFSET + kindInt;
					return sign * code;
				}

				default:
					throw new NotImplementedException($"Unknown SchedulePointType (int: {decode.kind})");
			}

			long Combine(int hr, int min, long sec)
			{
				return hr * TimeUtility.SECS_IN_ONE_HOUR +
					min * TimeUtility.SECS_IN_ONE_MINUTE +
					sec;
			}

			long ToWeeklyOffset(in SchedulePointDecode decode)
			{
				return decode.day * DAY_OF_WEEK_OFFSET +
					Combine(decode.hr, decode.min, decode.sec);
			}
		}

		public static SchedulePointDecode GetDefault(SchedulePointKind kind) =>
			kind switch
			{
				SchedulePointKind.Interval => new(kind) {sec = 1, sign = true},

				SchedulePointKind.Date => new(kind)
				{
					yr = DateTime.UtcNow.Year,
					mh = 0,
					day = 0,
					hr = 9,
					min = 0,
					sec = 0,
					sign = true
				},

				SchedulePointKind.Daily => new(kind) {hr = 9, min = 0, sec = 0, sign = true},
				SchedulePointKind.Monthly => new(kind) {day = 1, hr = 9, min = 0, sec = 0, sign = true},
				SchedulePointKind.Yearly => new(kind) {mh = 0, day = 1, hr = 9, min = 0, sec = 0, sign = true},

				SchedulePointKind.Weekly => new(kind) {day = 0, hr = 9, min = 0, sec = 0, sign = true},
				SchedulePointKind.MonthlyOnWeekday => new(kind) {day = 0, hr = 9, min = 0, sec = 0, weekOfMonth = 0, sign = true},
				SchedulePointKind.YearlyOnWeekday => new(kind) {day = 0, hr = 9, min = 0, sec = 0, weekOfMonth = 0, mh = 0, sign = true},

				_ => new(kind) {yr = 1970, mh = 0, day = 0, hr = 0, min = 0, sec = 0, sign = true}
			};
	}
}
