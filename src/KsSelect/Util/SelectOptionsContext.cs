using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Kapusons.Components.Util
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public class SelectOptionsContext<TEntity>
		 where TEntity : new()
	{
		/// <summary>
		/// 
		/// </summary>
		public SelectOptionsContext() { }

		/// <summary>
		/// 
		/// </summary>
		public SelectOptions SelectOptions { get; private set; }

		internal Func<SelectOptions, IQueryable<TEntity>, IQueryable<TEntity>> SelectOptionsProvider { get; private set; }

		internal Type ColumnExpressionsInputType { get; private set; }

		internal SelectOptionsContext<TEntity> Next { get; private set; }

		// for internal join projections
		/// <summary>
		/// Crea un contesto figlio per l'esecuzione di una proiezione basata su una derivazione (es. join) della query.
		/// </summary>
		/// <param name="provider">
		/// Delegato da invocare per l'esecuzione della proiezione.
		/// Il delegato riceve in input i parametri della proiezione (<see cref="SelectOptions"/>) e la query di origine (<see cref="IQueryable{TEntity}"/>),
		/// attraverso la quale potrà effettuare la derivazione, eseguire la proiezione (utilizzando <see cref="LinqUtil.Select{TEntity, TResult}(IQueryable{TEntity}, SelectOptions)"/>),
		/// e infine restituire una query compatibile con quella di input (la trasformazione effettuata non sarà visibile all'esterno).
		/// </param>
		/// <param name="columnExpressionsInputType"></param>
		/// <param name="sourceParentSelector"></param>
		/// <param name="inheritColumnFlags"></param>
		/// <param name="maxDepth">Massima profondità con cui dovrà essere effettuata la mappatura verso le proprietà dell'entità di destinazione.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <remarks>
		/// Tutte le proiezioni definite saranno applicate cumulativamente chiamando
		/// <see cref="SelectOptionsContextExtensions.ApplySelectOptions{TEntity}(IQueryable{TEntity}, SelectOptionsContext{TEntity})"/> sulla query interessata.
		/// </remarks>
		/// <seealso cref="UseSelectOptions(int?)"/>
		public SelectOptionsContext<TEntity> UseScoped(Func<SelectOptions, IQueryable<TEntity>, IQueryable<TEntity>> provider,
			Type columnExpressionsInputType, string sourceParentSelector,
			bool inheritColumnFlags = true,
			int? maxDepth = null)
		{
			if (provider is null) throw new ArgumentNullException(nameof(provider));

			var options = CreateOptions(maxDepth,
				inheritColumnFlags ? SelectOptions?.ColumnsToInclude : null,
				inheritColumnFlags ? SelectOptions?.ColumnsToExclude : null);

			if (!string.IsNullOrEmpty(sourceParentSelector)) options.SourceRootSelector = sourceParentSelector;

			if (SelectOptions is null) return SetupScopedContext(this, options, provider, columnExpressionsInputType);

			var nestedContext = SetupScopedContext(new SelectOptionsContext<TEntity>(), options, provider, columnExpressionsInputType);

			//Next = nestedContext;
			var current = this;
			while (current.Next != null) current = current.Next;
			current.Next = nestedContext;

			return nestedContext;
		}
		private SelectOptionsContext<TEntity> SetupScopedContext(SelectOptionsContext<TEntity> context, SelectOptions options, Func<SelectOptions, IQueryable<TEntity>, IQueryable<TEntity>> provider, Type columnExpressionsInputType)
		{
			context.SelectOptions = options;
			context.SelectOptionsProvider = provider;
			context.ColumnExpressionsInputType = columnExpressionsInputType ?? typeof(TEntity);
			return context;
		}

		/// <summary>
		/// Crea un contesto figlio per l'esecuzione di una proiezione basata su una derivazione (es. join) della query.
		/// </summary>
		/// <param name="provider">
		/// Delegato da invocare per l'esecuzione della proiezione.
		/// Il delegato riceve in input i parametri della proiezione (<see cref="SelectOptions"/>) e la query di origine (<see cref="IQueryable{TEntity}"/>),
		/// attraverso la quale potrà effettuare la derivazione, eseguire la proiezione (utilizzando <see cref="LinqUtil.Select{TEntity, TResult}(IQueryable{TEntity}, SelectOptions)"/>),
		/// e infine restituire una query compatibile con quella di input (la trasformazione effettuata non sarà visibile all'esterno).
		/// </param>
		/// <param name="sourceToTargetSelector"></param>
		/// <param name="inheritColumnFlags"></param>
		/// <param name="maxDepth">Massima profondità con cui dovrà essere effettuata la mappatura verso le proprietà dell'entità di destinazione.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <remarks>
		/// Tutte le proiezioni definite saranno applicate cumulativamente chiamando
		/// <see cref="SelectOptionsContextExtensions.ApplySelectOptions{TEntity}(IQueryable{TEntity}, SelectOptionsContext{TEntity})"/> sulla query interessata.
		/// </remarks>
		/// <seealso cref="UseSelectOptions(int?)"/>
		public SelectOptionsContext<TEntity> UseScoped<T>(Func<SelectOptions<T, TEntity>, IQueryable<TEntity>, IQueryable<T>> provider,
			Expression<Func<T, TEntity>> sourceToTargetSelector,
			bool inheritColumnFlags = true,
			int? maxDepth = null)
		{
			if (provider is null) throw new ArgumentNullException(nameof(provider));

			string sourceToTargetSelectorString =
				sourceToTargetSelector != null
				 ? ExpressionUtil.GetModelName(sourceToTargetSelector)
				 : null;

			//return UseScoped(provider, typeof(T), sourceToTargetSelectorString, inheritColumnFlags, maxDepth);
			return UseScoped<T>((options, q0) => { var q1 = provider(options, q0); return q1.Select<T, TEntity>(options); }, typeof(T), sourceToTargetSelectorString, inheritColumnFlags, maxDepth);
		}
		private SelectOptionsContext<TEntity> UseScoped<T>(Func<SelectOptions<T, TEntity>, IQueryable<TEntity>, IQueryable<TEntity>> provider,
			Type columnExpressionsInputType, string sourceParentSelector,
			bool inheritColumnFlags = true,
			int? maxDepth = null)
		{
			if (provider is null) throw new ArgumentNullException(nameof(provider));

			var options = CreateOptions<T, TEntity>(maxDepth,
				inheritColumnFlags ? SelectOptions?.ColumnsToInclude : null,
				inheritColumnFlags ? SelectOptions?.ColumnsToExclude : null);

			if (!string.IsNullOrEmpty(sourceParentSelector))
			{
				options.SourceRootSelector = sourceParentSelector;
			}

			if (SelectOptions is null)
			{
				return SetupScopedContext(this);
			}

			var nestedContext = SetupScopedContext(new SelectOptionsContext<TEntity>());

			//Next = nestedContext;
			var current = this;
			while (current.Next != null) current = current.Next;
			current.Next = nestedContext;

			return nestedContext;

			#region Local functions
			SelectOptionsContext<TEntity> SetupScopedContext(SelectOptionsContext<TEntity> context)
			{
				context.SelectOptions = options;
				context.SelectOptionsProvider = (o, q0) => provider(/*(SelectOptions<T, TEntity>)o*/options, q0);
				context.ColumnExpressionsInputType = columnExpressionsInputType ?? typeof(TEntity);
				return context;
			}
			#endregion Local functions
		}

		/// <summary>
		/// Ottiene le opzioni di configurazione per una proiezione attraverso <c>Select(options)</c>.
		/// </summary>
		/// <param name="maxDepth"></param>
		/// <returns></returns>
		/// <remarks>
		/// Tutte le proiezioni definite saranno applicate cumulativamente chiamando
		/// <see cref="SelectOptionsContextExtensions.ApplySelectOptions{TEntity}(IQueryable{TEntity}, SelectOptionsContext{TEntity})"/> sulla query interessata.
		/// </remarks>
		/// <seealso cref="UseScoped{T}(Func{SelectOptions{T, TEntity}, IQueryable{TEntity}, IQueryable{TEntity}}, Type, string, bool, int?)"/>
		public SelectOptions<TEntity> UseSelectOptions(int? maxDepth = null)
		{
			// find the first node with no custom SelectOptionsProvider
			var current = this;
			while (current.SelectOptionsProvider != null)
			{
				// tail node: append a new empty context
				if (current.Next is null) current.Next = new SelectOptionsContext<TEntity>();

				current = current.Next;
			}
			// ensure SelectOptions available
			current.SelectOptions = current.SelectOptions ?? CreateOptions<TEntity>(maxDepth);

			return (SelectOptions<TEntity>)current.SelectOptions;
		}

		private SelectOptions<T1, T2> CreateOptions<T1, T2>(int? maxDepth = null, IEnumerable<string> columnsToInclude = null, IEnumerable<string> columnsToExclude = null)
		{
			var options = new SelectOptions<T1, T2>();
			SetupOptions(options, maxDepth, columnsToInclude, columnsToExclude);
			return options;
		}
		private SelectOptions<T> CreateOptions<T>(int? maxDepth = null, IEnumerable<string> columnsToInclude = null, IEnumerable<string> columnsToExclude = null)
		{
			var options = new SelectOptions<T>();
			SetupOptions(options, maxDepth, columnsToInclude, columnsToExclude);
			return options;
		}
		private SelectOptions CreateOptions(int? maxDepth = null, IEnumerable<string> columnsToInclude = null, IEnumerable<string> columnsToExclude = null)
		{
			var options = new SelectOptions();
			SetupOptions(options, maxDepth, columnsToInclude, columnsToExclude);
			return options;
		}
		private void SetupOptions(SelectOptions options, int? maxDepth, IEnumerable<string> columnsToInclude, IEnumerable<string> columnsToExclude)
		{
			if (maxDepth.HasValue) options.MaxDepth = maxDepth.Value;

			if (columnsToInclude != null) options.ColumnsToInclude.AddRange(columnsToInclude);

			if (columnsToExclude != null) options.ColumnsToExclude.AddRange(columnsToExclude);
			else
			{
				options.ColumnsToExclude.AddRange(ColumnsToExcludeProvider?.Invoke(typeof(TEntity))
					.ValidateColumnNames(ExcludeNavigationPropertiesValidator) ?? Enumerable.Empty<string>()
					/*.ToHashSet()*/);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Func<Type, IEnumerable<string>> ColumnsToExcludeProvider { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Func<string, bool> ExcludeNavigationPropertiesValidator { get; set; }
	}
	/// <summary>
	/// 
	/// </summary>
	public static class SelectOptionsContextExtensions
	{
		internal static IEnumerable<string> ValidateColumnNames(this IEnumerable<string> columnNames, Func<string, bool> columnValidator)
		{
			if (columnNames == null) throw new ArgumentNullException(nameof(columnNames));

			if (columnValidator is null) return columnNames;
			return columnNames.Where(column => columnValidator(column));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="query"></param>
		/// <param name="filterContext"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IQueryable<TEntity> ApplySelectOptions<TEntity>(this IQueryable<TEntity> query, SelectOptionsContext<TEntity> filterContext)
			where TEntity : new()
		{
			if (query is null) throw new ArgumentNullException(nameof(query));
			if (filterContext is null) throw new ArgumentNullException(nameof(filterContext));

			var currentContext = filterContext;
			SelectOptions parentOptions = null;
			do
			{
				if (currentContext.SelectOptions != null)
				{
					var options = currentContext.SelectOptions;
					if (parentOptions != null) options.InherithFrom(parentOptions,
						currentContext.ColumnExpressionsInputType ?? typeof(TEntity),
						options.SourceRootSelector);

					if (currentContext.SelectOptionsProvider != null)
					{
						query = currentContext.SelectOptionsProvider(options, query);
					}
					else query = query.Select(options);
				}
				parentOptions = currentContext.SelectOptions;
				currentContext = currentContext.Next;
			}
			while (currentContext != null);

			return query;
		}
	}
}
