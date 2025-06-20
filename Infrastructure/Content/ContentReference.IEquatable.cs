using System;

namespace Content
{
	public partial struct ContentReference<T> : IEquatable<ContentReference<T>>
	{
		public bool Equals(ContentReference<T> other) => guid.Equals(other.guid);

		public override bool Equals(object obj) => obj is ContentReference<T> other && Equals(other);
	}

	public partial struct ContentReference : IEquatable<ContentReference>
	{
		public bool Equals(ContentReference other) => guid.Equals(other.guid);

		public override bool Equals(object obj) => obj is ContentReference other && Equals(other);
	}
}
