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
		private const long WEEKLY_OFFSET = 100000L;

		private const long MONTHLY_OFFSET = 100000L;

		private const long MONTH_YEARLY_OFFSET = 10000000L;
		private const long DAY_YEARLY_OFFSET = 100000L;
		private const long YEARLY_OFFSET_3 = 100L;

		private const long DATE_OFFSET = 100000L;

		public bool sign;
		public SchedulePointKind kind;
		public long yr;
		public int mh;
		public long day;
		public int hr;
		public int min;
		public long sec;

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

				case SchedulePointKind.Weekly:
				{
					var day = offset / WEEKLY_OFFSET;
					var (hr, min, sec) = Split(offset % WEEKLY_OFFSET);
					return new SchedulePointDecode(kind) {day = day, hr = hr, min = min, sec = sec};
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
					var day = offset / DAY_YEARLY_OFFSET % YEARLY_OFFSET_3;
					var (hr, min, sec) = Split(offset % DAY_YEARLY_OFFSET);
					return new SchedulePointDecode(kind) {mh = (int) mh, day = day, hr = hr, min = min, sec = sec, sign = sign};
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
						mh = date.Month - 1,
						day = date.Day - 1,
						hr = hr,
						min = min,
						sec = sec
					};
				}

				default:
					throw new NotImplementedException($"Unknown SchedulePointType (int: {kind})");
			}

			(int hr, int min, int sec) Split(long seconds)
			{
				var hr = seconds / TimeUtility.SECS_IN_ONE_HOUR % 24;
				var min = (seconds / TimeUtility.MINS_IN_ONE_HOUR)
					% TimeUtility.SECS_IN_ONE_MINUTE;
				var sec = seconds % TimeUtility.SECS_IN_ONE_MINUTE;
				return ((int) hr, (int) min, (int) sec);
			}
		}

		public static SchedulePointCode Encode(in SchedulePointDecode decode)
		{
			var kindInt = decode.kind.ToInt();
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

				case SchedulePointKind.Weekly:
				{
					return (decode.day * WEEKLY_OFFSET +
						Combine(decode.hr, decode.min, decode.sec)) * ISchedulePoint.TYPE_OFFSET + kindInt;
				}

				case SchedulePointKind.Monthly:
				{
					var sign = decode.sign ? 1 : -1;
					return sign * ((decode.day * MONTHLY_OFFSET +
							Combine(decode.hr, decode.min, decode.sec))
						* ISchedulePoint.TYPE_OFFSET + kindInt);
				}

				case SchedulePointKind.Yearly:
				{
					var sign = decode.sign ? 1 : -1;
					return sign * ((decode.mh * MONTH_YEARLY_OFFSET +
						decode.day * DAY_YEARLY_OFFSET +
						Combine(decode.hr, decode.min, decode.sec)) * ISchedulePoint.TYPE_OFFSET + kindInt);
				}

				case SchedulePointKind.Date:
				{
					var date = new DateTime(
						(int) decode.yr,
						decode.mh + 1,
						(int) decode.day + 1, 0, 0, 0, DateTimeKind.Utc);
					var day = (date - DateTime.UnixEpoch).Days;
					return
						(day * DATE_OFFSET +
							Combine(decode.hr, decode.min, decode.sec)
						)
						* ISchedulePoint.TYPE_OFFSET + kindInt;
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
		}

		public static SchedulePointDecode GetDefault(SchedulePointKind kind) =>
			kind switch
			{
				SchedulePointKind.Interval => new() {kind = kind, sec = 1},
				SchedulePointKind.Daily => new() {kind = kind, hr = 9, min = 0, sec = 0},
				SchedulePointKind.Weekly => new() {kind = kind, day = 0, hr = 9, min = 0, sec = 0},
				SchedulePointKind.Monthly => new() {kind = kind, day = 1, hr = 9, min = 0, sec = 0},
				SchedulePointKind.Yearly => new() {kind = kind, mh = 0, day = 1, hr = 9, min = 0, sec = 0},
				SchedulePointKind.Date => new()
				{
					kind = kind,
					yr = DateTime.UtcNow.Year,
					mh = 0,
					day = 0,
					hr = 9,
					min = 0,
					sec = 0
				},

				_ => new() {kind = kind, yr = 1970, mh = 0, day = 0, hr = 0, min = 0, sec = 0}
			};
	}
}
