using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Sapientia.Reflection
{
	public static class FastReflection
	{
		#region Generic

		private static readonly ConcurrentDictionary<RawGenericPairKey, Type> _genericPairTypeToCacheGenericType = new();

		public static Type WithParameter(this Type genericDefinition, Type genericParameter) =>
			_genericPairTypeToCacheGenericType.GetOrAdd(new(genericDefinition, genericParameter),
				genericDefinition.MakeGenericType(genericParameter));

		private readonly struct RawGenericPairKey : IEquatable<RawGenericPairKey>
		{
			private readonly Type definition;
			private readonly Type parameter;

			public RawGenericPairKey(Type definition, Type parameter)
			{
				this.definition = definition;
				this.parameter = parameter;
			}

			public bool Equals(RawGenericPairKey other) =>
				definition == other.definition && parameter == other.parameter;

			public override bool Equals(object obj) => obj is RawGenericPairKey other && Equals(other);

			public override int GetHashCode() => HashCode.Combine(definition, parameter);
		}

		#endregion

		#region FieldInfo

		private static BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
		private static ConcurrentDictionary<RawFieldInfoKey, FieldInfo> _fieldToInfo = new();

		public static void Clear()
		{
			_genericPairTypeToCacheGenericType.Clear();
			_fieldToInfo.Clear();
		}

		public static object GetReflectionValue(this object obj, string name)
			=> GetFieldInfo(obj.GetType(), name).GetValue(obj);

		public static object GetValue(this Type type, string name, object obj)
			=> GetFieldInfo(type, name).GetValue(obj);

		public static object GetReflectionValueSafe(this object obj, string name)
			=> GetFieldInfo(obj.GetType(), name)?.GetValue(obj);

		public static object GetValueSafe(this Type type, string name, object obj)
			=> GetFieldInfo(type, name)?.GetValue(obj);

		public static void SetValue(this Type type, string name, object obj, object value)
			=> GetFieldInfo(type, name).SetValue(obj, value);

		public static void SetValueSafe(this Type type, string name, object obj, object value)
			=> GetFieldInfo(type, name)?.SetValue(obj, value);

		private static FieldInfo GetFieldInfo(this Type type, string name)
			=> _fieldToInfo.GetOrAdd(new(type, name),
				type.GetField(name, _bindingFlags));

		private readonly struct RawFieldInfoKey : IEquatable<RawFieldInfoKey>
		{
			private readonly Type type;
			private readonly string name;

			public RawFieldInfoKey(Type type, string name)
			{
				this.type = type;
				this.name = name;
			}

			public bool Equals(RawFieldInfoKey other) => type == other.type && name == other.name;
			public override bool Equals(object obj) => obj is RawFieldInfoKey other && Equals(other);

			public override int GetHashCode() => HashCode.Combine(type, name);
		}

		#endregion
	}
}
