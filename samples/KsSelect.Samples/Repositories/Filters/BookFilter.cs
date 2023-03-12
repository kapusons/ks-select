using KsSelect.Samples.Repositories.Filters;

namespace KsSelect.Samples.Repositories.Filter;

public class BookFilter : BaseQueryParameters
{
	public bool IncludeAuthorName { get; set; }

	public bool IncludeAuthorInfo { get; set; }

	public string? Title { get; set; }
}
