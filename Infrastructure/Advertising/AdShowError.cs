using System;

namespace Advertising
{
	public enum AdShowErrorCode
	{
		None,

		NotFoundPlacementEntry,

		NotInitialized,
		NotReady,
		NotLoaded,

		NotImplementedPlacementType,

		UsageLimit
	}

	public readonly struct AdShowError : IEquatable<AdShowError>
	{
		public readonly AdShowErrorCode code;

		/// <summary>
		/// Сырые данные из интеграции
		/// </summary>
#if CLIENT
		[JetBrains.Annotations.CanBeNull]
#endif
		public readonly object rawData;

		public AdShowError(AdShowErrorCode code, object rawData = null)
		{
			this.code = code;
			this.rawData = rawData;
		}

		public static implicit operator AdShowError(AdShowErrorCode code) => new AdShowError(code);
		public static implicit operator AdShowErrorCode(AdShowError error) => error.code;

		public override string ToString() => rawData == null ? code.ToString() : $"{code} {rawData}";

		public bool Equals(AdShowError other) => code == other.code && Equals(rawData, other.rawData);

		public override bool Equals(object obj) => obj is AdShowError other && Equals(other);

		public override int GetHashCode() => HashCode.Combine((int) code, rawData);
	}
}
