namespace KsSelect.Samples.Models;

public interface IEntity
{
	long Id { get; set; }

}

public abstract class Entity : IEntity
{
	public long Id { get; set; }
}
