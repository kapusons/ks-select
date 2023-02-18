using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Kapusons.Components
{
	/// <summary>
	/// Provides extensions methods for <see cref="SerializationInfo"/>.
	/// </summary>
	public static class SerializationInfoExtensions
	{
		/// <summary>
		/// Retrieves a typed value from the <see cref="SerializationInfo"/> store.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="serializationInfo"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static T GetValue<T>(this SerializationInfo serializationInfo,string name)
		{
			if (serializationInfo==null) throw new ArgumentNullException("serializationInfo");
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

			return (T)serializationInfo.GetValue(name,typeof(T));
		}

		/// <summary>
		/// Adds a value into the <see cref="SerializationInfo"/> store, where value is associated with <c>name</c>
		/// and is serialized as being of <c>T</c> type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="serializationInfo"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public static void AddTypedValue<T>(this SerializationInfo serializationInfo,string name,T value)
		{
			if (serializationInfo==null) throw new ArgumentNullException("serializationInfo");
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

			serializationInfo.AddValue(name,value,typeof(T));
		}
	}
}
