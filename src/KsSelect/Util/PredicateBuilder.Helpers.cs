using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kapusons.Components.Util
{
	partial class PredicateBuilder
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="target"></param>
		/// <param name="collection"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> collection)
		{
			if (target is null) throw new ArgumentNullException(nameof(target));
			if (collection is null) throw new ArgumentNullException(nameof(collection));

			foreach (var item in collection) target.Add(item);
		}

		internal static bool HasNotMappedAttributes(this MemberInfo property, bool inherit = false)
		{
			if (property is null) throw new ArgumentNullException(nameof(property));

			var notMappedType = typeof(NotMappedAttribute);
			var ksNotMappedType = typeof(KsNotMappedAttribute);
			return property.GetCustomAttributes(inherit).Any(it => notMappedType.IsAssignableFrom(it.GetType())
				|| ksNotMappedType.IsAssignableFrom(it.GetType()));
		}

		internal static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
		{
			if (dictionary == null) throw new ArgumentNullException("dictionary");

			TValue value;
			dictionary.TryGetValue(key, out value);
			return value;
		}
	}
}
