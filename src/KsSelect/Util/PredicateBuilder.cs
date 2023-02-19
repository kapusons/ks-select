using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Kapusons.Components.Util
{
	// https://github.com/EdCharbeneau/PredicateExtensions
	// https://www.simple-talk.com/dotnet/.net-framework/giving-clarity-to-linq-queries-by-extending-expressions/
	// https://www.nuget.org/packages/PredicateExtensions/1.0.0

	// see also:
	// https://www.nuget.org/packages/System.Linq.Dynamic/1.0.4
	// https://www.nuget.org/packages/System.Linq.Dynamic.Library/1.1.14
	// http://dynamiclinq.azurewebsites.net/Expressions
	// http://www.albahari.com/nutshell/predicatebuilder.aspx
	// http://www.albahari.com/nutshell/linqkit.aspx
	// http://tomasp.net/blog/dynamic-linq-queries.aspx/


	// Partially based on: Adam Tegen via StackOverflow http://stackoverflow.com/questions/457316/combining-two-expressions-expressionfunct-bool
	// Modified by Ed Charbeneau

	#region Support types (SelectOptions, SelectMappingAttribute)
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SelectOptions<T> : SelectOptions<T, T> { }
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TResult"></typeparam>
	public class SelectOptions<T, TResult> : SelectOptions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <param name="columnSelector"></param>
		/// <param name="columnValueExpression"></param>
		/// <returns></returns>
		public SelectOptions<T, TResult> Include<T1>(Expression<Func<TResult, T1>> columnSelector, Expression<Func<T, T1>> columnValueExpression)
		{
			var key = ExpressionUtil.GetModelName(columnSelector);
			ColumnExpressions[key] = columnValueExpression;
			return this;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <param name="columnSelector"></param>
		/// <returns></returns>
		public SelectOptions<T, TResult> Include<T1>(Expression<Func<TResult, T1>> columnSelector)
		{
			var key = ExpressionUtil.GetModelName(columnSelector);
			ColumnsToInclude.Add(key);
			return this;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <param name="columnSelector"></param>
		/// <returns></returns>
		public SelectOptions<T, TResult> Exclude<T1>(Expression<Func<TResult, T1>> columnSelector)
		{
			var key = ExpressionUtil.GetModelName(columnSelector);
			ColumnsToExclude.Add(key);
			return this;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class SelectOptions
	{
		/// <summary>
		/// 
		/// </summary>
		public const int DefaultMaxDepth = 1;

		/// <summary>
		/// 
		/// </summary>
		public ISet<string> ColumnsToInclude { get; set; } = new HashSet<string>();

		/// <summary>
		/// 
		/// </summary>
		public ISet<string> ColumnsToExclude { get; set; } = new HashSet<string>();

		/// <summary>
		/// 
		/// </summary>
		public IDictionary<string, Expression> ColumnExpressions { get; set; } = new Dictionary<string, Expression>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="columnExpressionsInputType"></param>
		/// <param name="sourceParentSelector">Selettore per giungere dall'entità di input attuale all'entità di input precedente.</param>
		public void InherithFrom(SelectOptions parent, Type columnExpressionsInputType, string sourceParentSelector)
		{
			ColumnsToInclude.AddRange(parent.ColumnsToInclude);
			ColumnsToExclude.AddRange(parent.ColumnsToExclude);
			foreach (var parentColumn in parent.ColumnExpressions)
			{
				if (ColumnExpressions.ContainsKey(parentColumn.Key)) continue;

				// copiamo il valore della colonna calcolata con il precedente SelectOptions attraverso la seguente lambda expression:
				// ([columnExpressionsInputType] it) => it.[parentSourceSelector].[key]

				var inputParameter = Expression.Parameter(columnExpressionsInputType, "it");
				Expression memberAccessExpression = inputParameter;
				if (!string.IsNullOrEmpty(sourceParentSelector)) memberAccessExpression = Expression.Property(memberAccessExpression, sourceParentSelector);
				// multipart key processing ([propertyName].[propertyName]...)
				foreach (var keyPart in parentColumn.Key.Split('.'))
				{
					memberAccessExpression = Expression.Property(memberAccessExpression, keyPart);
				}

				var columnExpression = Expression.Lambda(memberAccessExpression, inputParameter);
				ColumnExpressions.Add(parentColumn.Key, columnExpression);
			}
		}

		///// <summary>
		///// 
		///// </summary>
		//public Expression SourceRootSelectorExpression { get; set; }
		/// <summary>
		/// Permette di specificare il membro dell'entità sorgente relativamente
		/// al quale vengono cercate le corrispondenze con i membri dell'entità di destinazione.
		/// </summary>
		/// <remarks>
		/// Non viene utilizzato per le column expression, che ricevono sempre in input l'oggetto sorgente radice.
		/// </remarks>
		public string SourceRootSelector { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int MaxDepth { get; set; } = DefaultMaxDepth;
	}

	/// <summary>
	/// 
	/// </summary>
	public class SelectOptionsMappingAttribute : Attribute
	{
		//public SelectOptionsMappingAttribute() { }

		/// <summary>
		/// 
		/// </summary>
		public bool Ignore { get; set; }

		///// <summary>
		///// 
		///// </summary>
		//public bool Include { get; set; }

		/// <summary>
		/// Map inner properties of complex type.
		/// </summary>
		public bool IncludeChildren { get; set; }

		/// <summary>
		/// The member is mapped only if a corresponding column expression is present.
		/// </summary>
		public bool RequireColumnExpression { get; set; }

		// non supportato: l'attuale implementazione itera sui membri di destinazione,
		// mentre questo attributo dovrebbe essere applicato a un membro sorgente
		///// <summary>
		///// Map the inner properties to the outer target object.
		///// </summary>
		//public bool MapToOuterType { get; set; }

		/// <summary>
		/// Do not create a new instance of the target container object.
		/// </summary>
		public bool UseExistingTargetComplexTypeInstance { get; set; }
	}
	#endregion Support types

	/// <summary>
	/// 
	/// </summary>
	public static partial class PredicateBuilder
	{
		#region SelectOptions
		private class SelectOptionsItem
		{
			public string BaseKey { get; set; }

			public string Key => BaseKey != null ? BaseKey + '.' + Name : Name;

			public string Name => Property.Name;

			public int Depth { get; set; }

			public PropertyInfo Property { get; set; }

			public SelectOptionsMappingAttribute Mapping { get; set; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static IQueryable<T> Select<T>(this IQueryable<T> query, SelectOptions options)
			=> query.Select<T, T>(options);

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query"></param>
		/// <param name="configure"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IQueryable<T> Select<T>(this IQueryable<T> query, Action<SelectOptions<T>> configure)
		{
			if (query is null) throw new ArgumentNullException(nameof(query));
			if (configure is null) throw new ArgumentNullException(nameof(configure));

			var options = new SelectOptions<T>();
			configure.Invoke(options);
			return query.Select<T, T>(options);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="query"></param>
		/// <param name="configure"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IQueryable<TResult> Select<T, TResult>(this IQueryable<T> query, Action<SelectOptions<T, TResult>> configure)
		{
			if (query is null) throw new ArgumentNullException(nameof(query));
			if (configure is null) throw new ArgumentNullException(nameof(configure));

			var options = new SelectOptions<T, TResult>();
			configure.Invoke(options);
			return query.Select<T, TResult>(options);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="query"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static IQueryable<TResult> Select<T, TResult>(this IQueryable<T> query, SelectOptions options)
			=> query.Select(GetSelectOptionsExpression<T, TResult>(options));

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="options"></param>
		/// <returns></returns>
		public static Expression<Func<T, TResult>> GetSelectOptionsExpression<T, TResult>(SelectOptions options)
		{
			if (options is null) throw new ArgumentNullException(nameof(options));

			var sourceRootType = typeof(T);
			var sourceType = sourceRootType;
			var targetType = typeof(TResult);
			var rootParameter = Expression.Parameter(sourceRootType, "source");
			Expression parameter = rootParameter;
			if (!string.IsNullOrEmpty(options.SourceRootSelector))
			{
				parameter = Expression.Property(parameter, options.SourceRootSelector);
				sourceType = sourceType.GetProperty(options.SourceRootSelector)?.PropertyType;
			}
			// NOTA: vengono enumerati i membri dal tipo di destinazione, NON dal tipo sorgente,
			// poichè vengono mappati (dalla sorgente) solo i membri disponibili nella destinazione.
			// Conseguentemente SelectOptionsItem, ottenuto dall'enumerazione principale, contiene informazioni relative a quest'ultima
			var bindings = typeof(TResult)
				.GetMemberBindings(options,
					baseKey: null,
					rootParameter: rootParameter,
					parameter: parameter,
					depth: 0,
					rootSourceType: sourceRootType,
					sourceType: sourceType);
			var newExpression = Expression.New(targetType/*.GetConstructor(Type.EmptyTypes)*/);
			var memberInitExpression = Expression.MemberInit(newExpression, bindings);

			return Expression.Lambda<Func<T, TResult>>(memberInitExpression, new[] { rootParameter });
		}

		private static MemberBinding GetMemberBinding(this SelectOptionsItem item,
			ParameterExpression rootParameter, Expression parameter,
			SelectOptions options, Type sourceType = null, Type rootSourceType = null)
		{
			if (item is null) throw new ArgumentNullException(nameof(item));
			if (rootParameter is null) throw new ArgumentNullException(nameof(rootParameter));
			if (options is null) throw new ArgumentNullException(nameof(options));

			var memberKey = item.Key;
			var memberName = item.Name;
			var customExpression = options.ColumnExpressions.GetValueOrDefault(memberKey)?.UnwrapLambda(rootParameter);
			var sourceProperty = sourceType?.GetProperty(memberName);

			parameter = parameter ?? rootParameter;
			var targetProperty = item.Property;

			//parameter = customExpression ?? Expression.Property(parameter, memberName);
			if (sourceProperty != null) parameter = Expression.Property(parameter, memberName);
			else parameter = Expression.Constant(null);

			if (item.Mapping?.IncludeChildren == true && item.Depth < options.MaxDepth)
			{
				// recursive path
				var targetType = targetProperty.PropertyType;
				var childBindings = targetType.GetMemberBindings(options, memberKey, rootParameter, parameter,
					item.Depth + 1, rootSourceType, sourceProperty?.PropertyType);

				if (item.Mapping?.UseExistingTargetComplexTypeInstance == true && sourceProperty != null)
				{
					// use existing => MemberBind(sourceProperty, bindings)
					return Expression.MemberBind(sourceProperty, childBindings);
				}
				else
				{
					// create new => Bind(targetMember, MemberInit(New(targetType, bindings)))
					return Expression.Bind(targetProperty, Expression.MemberInit(Expression.New(targetType), childBindings));
				}
			}

			// se non è presente una customExpression da cui ottenere il valore da mappare, è necessario che sourceProperty sia valorizzata
			if (customExpression == null && sourceProperty == null) return null;

			// bind member => targetProperty=sourceExpression
			return Expression.Bind(targetProperty, customExpression ?? parameter);
		}
		private static IEnumerable<MemberBinding> GetMemberBindings(this Type type, SelectOptions options, string baseKey,
			ParameterExpression rootParameter, Expression parameter,
			int depth,
			Type rootSourceType, Type sourceType = null)
		{
			return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(it => it.CanWrite && it.GetSetMethod().IsPublic)
				.Select(it => new SelectOptionsItem
				{
					BaseKey = baseKey,
					Property = it,
					Depth = depth,
					Mapping = it.GetMappingOptions(),
				})
				.ApplyOptions(options)
				.Select(it => it.GetMemberBinding(rootParameter, parameter, options, sourceType, rootSourceType))
				.Where(it => it != null);
		}

		private static SelectOptionsMappingAttribute GetMappingOptions(this PropertyInfo property)
		{
			// se l'attributo [SelectOptionsMapping] è specificato esplicitamente, prevale su qualsiasi
			// altra impostazione;
			// altrimenti, se è il caso, ne viene generato uno automaticamente, in base alle caratteristiche del membro;
			// se non ci sono impostazioni specifiche da recepire nessun attributo viene restituito (return null)
			var options = property.GetCustomAttribute<SelectOptionsMappingAttribute>(inherit: false);
			if (options != null) return options;

			if (property.HasNotMappedAttributes(inherit: false))
			{
				return new SelectOptionsMappingAttribute { RequireColumnExpression = true };
			}

			return null;
		}

		private static IEnumerable<SelectOptionsItem> ApplyOptions(this IEnumerable<SelectOptionsItem> items, SelectOptions options)
		{
			if (items is null) throw new ArgumentNullException(nameof(items));
			if (options is null) throw new ArgumentNullException(nameof(options));

			// black-list
			if (options.ColumnsToExclude.Any()) items = items.Where(it => !options.ColumnsToExclude.Contains(it.Key));

			// white-list
			if (options.ColumnsToInclude.Any()) items = items.Where(it => options.ColumnsToInclude.Contains(it.Key));

			// ignore filter
			items = items.Where(it => !it.ShouldIgnore(options));

			return items;
		}
		private static bool ShouldIgnore(this SelectOptionsItem item, SelectOptions options)
		{
			if (item is null) throw new ArgumentNullException(nameof(item));
			if (options is null) throw new ArgumentNullException(nameof(options));

			if (item.Mapping != null)
			{
				if (item.Mapping.Ignore) return true;
				if (item.Mapping.RequireColumnExpression) return !options.ColumnExpressions.ContainsKey(item.Key);
			}
			return false;
		}
		#endregion SelectOptions

		/// <summary>
		/// Begin an expression chain.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value">Default return value if the chain is ended early.</param>
		/// <returns>A lambda expression stub.</returns>
		public static Expression<Func<T, bool>> New<T>(bool value = false)
		{
			if (value) return parameter => true; //value cannot be used in place of true/false

			return parameter => false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
			=> CombineLambdas(left, right, ExpressionType.AndAlso);

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
			=> CombineLambdas(left, right, ExpressionType.OrElse);

		#region Invoke
		// References:
		//	https://github.com/scottksmith95/LINQKit/blob/master/src/LinqKit.Core/Linq.cs
		//	https://github.com/scottksmith95/LINQKit
		//	http://tomasp.net/blog/dynamic-linq-queries.aspx/

		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<TResult>(this Expression<Func<TResult>> expr)
		{
			return expr.Compile().Invoke();
		}

		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<T1, TResult>(this Expression<Func<T1, TResult>> expr, T1 arg1)
		{
			return expr.Compile().Invoke(arg1);
		}

		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> expr, T1 arg1, T2 arg2)
		{
			return expr.Compile().Invoke(arg1, arg2);
		}

		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<T1, T2, T3, TResult>(
			this Expression<Func<T1, T2, T3, TResult>> expr, T1 arg1, T2 arg2, T3 arg3)
		{
			return expr.Compile().Invoke(arg1, arg2, arg3);
		}

		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<T1, T2, T3, T4, TResult>(
			this Expression<Func<T1, T2, T3, T4, TResult>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			return expr.Compile().Invoke(arg1, arg2, arg3, arg4);
		}

#if !(NET35 || NET40)
		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<T1, T2, T3, T4, T5, TResult>(
			this Expression<Func<T1, T2, T3, T4, T5, TResult>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			return expr.Compile().Invoke(arg1, arg2, arg3, arg4, arg5);
		}

		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<T1, T2, T3, T4, T5, T6, TResult>(
			this Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
		T6 arg6)
		{
			return expr.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
		}

		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<T1, T2, T3, T4, T5, T6, T7, TResult>(
			this Expression<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
		T6 arg6, T7 arg7)
		{
			return expr.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}

		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(
			this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
		T6 arg6, T7 arg7, T8 arg8)
		{
			return expr.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
		}

		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(
			this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
		T6 arg6, T7 arg7, T8 arg8, T9 arg9)
		{
			return expr.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
		}

		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(
			this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
		T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
		{
			return expr.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
		}

		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(
			this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
		T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
		{
			return expr.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
		}

		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(
			this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
		T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
		{
			return expr.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
		}

		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(
			this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
		T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
		{
			return expr.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
		}

		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(
			this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
		T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
		{
			return expr.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
		}

		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(
			this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
		T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
		{
			return expr.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
		}

		/// <summary>Compile and invoke.</summary>
		public static TResult Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(
			this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
		T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
		{
			return expr.Compile().Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
		}
#endif //!(NET35 || NET40)
		#endregion Invoke

		#region Expr/Func
		// References:
		//	https://github.com/scottksmith95/LINQKit/blob/master/src/LinqKit.Core/Linq.cs
		//	https://github.com/scottksmith95/LINQKit
		//	http://tomasp.net/blog/dynamic-linq-queries.aspx/

		/// <summary>
		/// Returns the given anonymous method as a lambda expression
		/// </summary>
		public static Expression<Func<TResult>> Expr<TResult>(Expression<Func<TResult>> expr) => expr;

		/// <summary>
		/// Returns the given anonymous method as a lambda expression
		/// </summary>
		public static Expression<Func<T, TResult>> Expr<T, TResult>(Expression<Func<T, TResult>> expr) => expr;

		/// <summary>
		/// Returns the given anonymous method as a lambda expression.
		/// </summary>
		public static Expression<Func<T1, T2, TResult>> Expr<T1, T2, TResult>(Expression<Func<T1, T2, TResult>> expr) => expr;

		/// <summary>
		/// Returns the given anonymous function as a Func delegate.
		/// </summary>
		public static Func<TResult> Func<TResult>(Func<TResult> expr) => expr;

		/// <summary>
		/// Returns the given anonymous function as a Func delegate.
		/// </summary>
		public static Func<T, TResult> Func<T, TResult>(Func<T, TResult> expr) => expr;

		/// <summary>
		/// Returns the given anonymous function as a Func delegate.
		/// </summary>
		public static Func<T1, T2, TResult> Func<T1, T2, TResult>(Func<T1, T2, TResult> expr) => expr;
		#endregion Expr/Func

		/// <summary>
		/// 
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="replacementParameter"></param>
		/// <returns></returns>
		public static LambdaExpression ReplaceParameter(this LambdaExpression expression, ParameterExpression replacementParameter)
		{
			if (expression is null) throw new ArgumentNullException(nameof(expression));
			if (replacementParameter is null) throw new ArgumentNullException(nameof(replacementParameter));

			if (expression.Parameters.Count < 1) return expression;

			var visitor = new SubstituteParameterVisitor();
			visitor.Sub[expression.Parameters[0]] = replacementParameter;
			return (LambdaExpression)visitor.Visit(expression);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="replacementParameter"></param>
		/// <returns></returns>
		public static Expression UnwrapLambda(this Expression expression, ParameterExpression replacementParameter)
		{
			if (expression is null) throw new ArgumentNullException(nameof(expression));
			if (replacementParameter is null) throw new ArgumentNullException(nameof(replacementParameter));

			if (!(expression is LambdaExpression lambda)) return expression;

			return lambda.ReplaceParameter(replacementParameter).Body;
		}

		#region Private
		private static Expression<Func<T, bool>> CombineLambdas<T>(this Expression<Func<T, bool>> left,
			Expression<Func<T, bool>> right, ExpressionType expressionType)
		{
			//Remove expressions created with Begin<T>()
			if (IsExpressionBodyConstant(left)) return (right);

			ParameterExpression p = left.Parameters[0];

			SubstituteParameterVisitor visitor = new SubstituteParameterVisitor();
			visitor.Sub[right.Parameters[0]] = p;

			Expression body = Expression.MakeBinary(expressionType, left.Body, visitor.Visit(right.Body));
			return Expression.Lambda<Func<T, bool>>(body, p);
		}

		private static bool IsExpressionBodyConstant<T>(Expression<Func<T, bool>> left)
			=> left.Body.NodeType == ExpressionType.Constant;

		internal class SubstituteParameterVisitor : ExpressionVisitor
		{
			public Dictionary<Expression, Expression> Sub = new Dictionary<Expression, Expression>();

			protected override Expression VisitParameter(ParameterExpression node)
			{
				if (Sub.TryGetValue(node, out var newValue)) return newValue;
				return node;
			}
		}
		#endregion Private
	}
}
