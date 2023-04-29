using System.Globalization;
using Kapusons.Components.Util;
using KsSelect.Samples.Infrastructure;
using KsSelect.Samples.Models;
using KsSelect.Samples.Repositories.Filter;

namespace KsSelect.Samples.Repositories;

public interface IBookRepository : IRepository<Book, BookFilter> { }

public class BookRepository : BaseRepository<Book, BookFilter>, IBookRepository
{
	private const string DefaultLanguage = "en";

	public BookRepository(ISampleDbContext context, ILogger<BookRepository> logger) : base(context, logger) { }

	protected override IQueryable<Book> GetBaseQuery(bool includeDeleted = false, BookFilter? queryContext = null) => Context.GetBooksQuery();

	protected override IQueryable<Book> ApplyFilter(IQueryable<Book> query, BookFilter parameters, SelectOptionsContext<Book> filterContext)
	{
		if (parameters.IncludeAuthorName)
		{
			var options = filterContext.UseSelectOptions();

			var authorsQuery = Context.GetAuthorsQuery();

			options.Include(it => it.AuthorName, it =>
				authorsQuery.Where(g => g.Id == it.AuthorId)
					.Select(it => it.FirstName + " " + it.LastName)
					.FirstOrDefault());
		}

		if (parameters.IncludeAuthorInfo)
		{
			filterContext.UseScoped<BookAuthorJoin>((options, q0) =>
			{
				var joinedQuery = q0.Join(Context.GetAuthorsQuery(),
					b => b.AuthorId,
					a => a.Id,
					(b, a) => new BookAuthorJoin { Book = b, Author = a });
				options.Include(book => book.AuthorInfo, it => new AuthorInfo
				{
					FirstName = it.Author.FirstName,
					LastName = it.Author.LastName,
					BookSells = Context.GetBooksQuery().Count(x => x.AuthorId == it.Author.Id),
				});
				return joinedQuery;
			}, it => it.Book);
		}

		if (CultureInfo.CurrentUICulture.Name != DefaultLanguage)
		{
			filterContext.UseScoped<BookLocalizationJoin>((options, q0) =>
			{
				var joinedQuery = q0.Join(Context.GetBooksLocalizationQuery(),
					b => new { BookId = b.Id, Language = CultureInfo.CurrentUICulture.Name },
					l => new { l.BookId, l.Language },
					(b, l) => new BookLocalizationJoin { Book = b, Localization = l });
				options.Include(book => book.Title, it => it.Localization.Title);
				options.Include(book => book.Description, it => it.Localization.Description);

				if (!string.IsNullOrEmpty(parameters.Title)) joinedQuery = joinedQuery.Where(it => it.Localization.Title.Contains(parameters.Title));

				return joinedQuery;
			}, it => it.Book);
		}
		else
		{
			if (!string.IsNullOrEmpty(parameters.Title)) query = query.Where(it => it.Title.Contains(parameters.Title));
		}

		return base.ApplyFilter(query, parameters, filterContext);
	}
}
