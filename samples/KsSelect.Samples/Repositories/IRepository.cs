using Kapusons.Components.Util;
using KsSelect.Samples.Infrastructure;
using KsSelect.Samples.Models;
using KsSelect.Samples.Repositories.Filters;
using Microsoft.EntityFrameworkCore;

namespace KsSelect.Samples.Repositories;

public interface IRepository { }

public interface IRepository<TEntity, TQuery> : IRepository<TEntity, long, TQuery>
	where TEntity : class
	where TQuery : IQueryParameters { }

public interface IRepository<TEntity, TKey, TQuery> : IRepository
	where TEntity : class
	where TQuery : IQueryParameters
{
	Task<TEntity?> GetByIdAsync(TKey id, bool required = false);
	IEnumerable<TEntity> Query(TQuery queryParameters);
}

public abstract class BaseRepository<TEntity, TQueryParameters> : IRepository<TEntity, TQueryParameters>
		where TEntity : Entity, new()
		where TQueryParameters : class, IQueryParameters, new()
{
	protected BaseRepository(
		ISampleDbContext context,
		ILogger logger)
	{
		this.Context = context;
		this.Logger = logger;
		this.Set = context.Set<TEntity>();
	}

	protected ISampleDbContext Context { get; }
	protected ILogger Logger { get; }
	protected DbSet<TEntity> Set { get; }

	protected virtual IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query, TQueryParameters parameters, SelectOptionsContext<TEntity> filterContext)
	{
		if (parameters.Id.HasValue) query = query.Where(it => it.Id == parameters.Id.Value);
		if (parameters.Ids?.Any() ?? false) query = query.Where(it => parameters.Ids.Contains(it.Id));

		return query;
	}

	public async virtual Task<TEntity?> GetByIdAsync(long id, bool required = false)
	{
		var item = await Set.FindAsync(id);

		if (item is null && required) throw new Exception($"An {typeof(TEntity).Name} with key={id} does not exist.");

		return item;
	}

	public virtual IEnumerable<TEntity> Query(TQueryParameters queryParameters)
		=> BuildQueryable(queryParameters);

	protected abstract IQueryable<TEntity> GetBaseQuery(bool includeDeleted = false, TQueryParameters? queryContext = null);

	private IQueryable<TEntity> BuildQueryable(TQueryParameters queryParameters)
	{
		var query = ApplyFilterCore(GetBaseQuery(queryContext: queryParameters), queryParameters);
		//query = await ApplyPagingAndSortingAsync(query, queryParameters);
		return query;
	}


	// Quando è attivo il SelectOptions (tipicamente in conseguenza dell'inclusione di una o più colonne calcolate),
	// tutte le proprietà di navigazione sono di default spente e il lazy load non è utilizzabile:
	// per poterle referenziare, senza ottenere un'InvalidOperationException,
	// occorre includerle esplicitamente attraverso le corrispettive proprietà presenti nei filtri.
	// Il metodo seguente è utilizzato dall'implementazione di ApplyFilterContext (che ha il compito di configurare il SelectOptions),
	// per identificare quali proprietà di navigazione vanno incluse (opt-in) nella nuova proiezione.
	// L'implementazione predefinita di questo metodo riconosce automaticamente le inclusioni attraverso le proprietà del filtro denominate secondo la convenzione:
	// Include<nome proprietà di navigazione>
	// Eventuali altre inclusioni che non seguano questa convenzione occorre vanno gestite attraverso un override di questo metodo.
	protected virtual bool ExcludeNavigationPropertiesValidator(string columnName, TQueryParameters parameters)
	{
		//return true;
		var property = typeof(TQueryParameters).GetProperty("Include" + columnName);
		if (property is null) return true;
		return !(((bool?)property.GetValue(parameters)) ?? false);
	}

	private IQueryable<TEntity> ApplyFilterCore(IQueryable<TEntity> query, TQueryParameters parameters)
	{
		var filterContext = CreateApplyFilterContext(columnName => ExcludeNavigationPropertiesValidator(columnName, parameters));
		query = ApplyFilter(query, parameters, filterContext);

		query = query.ApplySelectOptions(filterContext);

		return query;
	}

	protected SelectOptionsContext<TEntity> CreateApplyFilterContext(Func<string, bool> excludeNavigationPropertiesValidator)
		=> new SelectOptionsContext<TEntity> { ExcludeNavigationPropertiesValidator = excludeNavigationPropertiesValidator };

}
