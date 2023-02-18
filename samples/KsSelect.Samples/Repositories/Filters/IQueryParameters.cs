namespace KsSelect.Samples.Repositories.Filters;

public interface IQueryParameters
{
	long? Id { get; set; }

	IEnumerable<long>? Ids { get; set; }
}

public abstract class BaseQueryParameters : IQueryParameters
{
	protected BaseQueryParameters() { }

	public long? Id { get; set; }

	// se inizializzato il binding dei parametri asp.net non viene effettuato
	// (in .NET6 viene prodotto in NullReferenceException);
	// in alternativa occorre utilizzare IList<T>
	public IEnumerable<long>? Ids { get; set; } //= Enumerable.Empty<long>();
}
