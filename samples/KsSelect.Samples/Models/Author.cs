#nullable disable
namespace KsSelect.Samples.Models;

public class Author : Entity
{
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public ICollection<Book> Books { get; set; } = new List<Book>();
}
#nullable restore
