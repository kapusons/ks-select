#nullable disable
namespace KsSelect.Samples.Models;

public class BookLocalization
{
	public long Id { get; set; }

	public string Language { get; set; }

	public long BookId { get; set; }

	public Book Book { get; set; }

	public string Title { get; set; }

	public string Description { get; set; }
}
