namespace KsSelect.Samples.ViewModels;

public class BookViewModel
{
	public long Id { get; set; }
	
	public string? Title { get; set; }

	public string? Description { get; set; }

	public AuthorInfoViewModel? AuthorInfo { get; set; }
}
