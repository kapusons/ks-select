#nullable disable
using System.ComponentModel.DataAnnotations;

namespace KsSelect.Samples.Models;

public class Author : Entity
{
	[Required]
	public string FirstName { get; set; }

	[Required]
	public string LastName { get; set; }

	public ICollection<Book> Books { get; set; } = new List<Book>();
}
#nullable restore
