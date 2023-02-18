using System.ComponentModel.DataAnnotations.Schema;

#nullable disable
namespace KsSelect.Samples.Models;

public class Book : Entity
{
	public string Title { get; set; }

	public string Description { get; set; }

	public long AuthorId { get; set; }

	public virtual Author Author { get; set; }

	#region Not mapped
	[NotMapped]
	public string AuthorName { get; set; }

	[NotMapped]
	public AuthorInfo AuthorInfo { get; set; }
	#endregion Not mapped
}
#nullable restore