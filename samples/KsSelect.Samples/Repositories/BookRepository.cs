using Kapusons.Components.Util;
using KsSelect.Samples.Infrastructure;
using KsSelect.Samples.Models;
using KsSelect.Samples.Repositories.Filter;

namespace KsSelect.Samples.Repositories;

public interface IBookRepository : IRepository<Book, BookFilter> { }

public class BookRepository : BaseRepository<Book, BookFilter>, IBookRepository
{
	public BookRepository(ISampleDbContext context, ILogger<BookRepository> logger) : base(context, logger) { }

	protected override IQueryable<Book> GetBaseQuery(bool includeDeleted = false, BookFilter? queryContext = null) => Context.GetBooksQuery();

	protected override IQueryable<Book> ApplyFilter(IQueryable<Book> query, BookFilter parameters, SelectOptionsContext<Book> filterContext)
	{
		if (parameters.IncludeAuthorName)
		{
			var options = filterContext.UseSelectOptions();

			var authorsQuery = Context.GetAuthorsQuery();

			options.ColumnExpressions[nameof(Book.AuthorName)] = (Book it) =>
				authorsQuery.Where(g => g.Id == it.AuthorId)
					.Select(it => it.FirstName + " " + it.LastName)
					.FirstOrDefault();
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

		return base.ApplyFilter(query, parameters, filterContext);
	}
}
