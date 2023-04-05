# select-options (aka ks-select)
[![NuGet](https://img.shields.io/nuget/v/Kapusons.Select?label=NuGet)](https://www.nuget.org/packages/Kapusons.Select)

Augments linq `IQueryable` `Select` capabilities with configurable options to allow build composable projections, based on conditional constructs.
> works virtually with any linq provider and linq-based framework (linq to objects, ef6, ef-core, etc.), because it simply generates an expression tree for the `IQueryable` `Select` method.

---

	query.Select<Product, ProductInfo>(options =>
	{
		options.Include(it => it.Category, it => categoryQuery.Where(c => c.Id == it.CategoryId).Select(c => c.Name).FirstOrDefault());
		options.Exclude(it => it.Description);
	});

## Features

### Conditionally exclude or include columns

	if (excludeBinaryData) query = query.Select(options => { options.Exclude(it => it.BinaryData); });
	if (includeOrderCount) query = query.Select(options => { options.Include(it.OrderCount, it => orderQuery.Count(o => o.ProductId == it.Id)) });

or

	query = query.Select(options =>
	{
		if (excludeBinaryData) options.Exclude(it => it.BinaryData);
		if (includeOrderCount) options.Include(it.OrderCount, it => orderQuery.Count(o => o.ProductId == it.Id));
	});

> The extra columns can be applied either to the original entity (usually those columns are not mapped to the data store and are marked with the `[NotMapped]` attribute)
> or to a distinct type (see the demo project for an example)

### AutoMapper-like projection
Properties with a matching name are automatically mapped from the original type to the target type. Supports complex type properties.

### Provider agnostic
Works virtually with any linq provider and linq-based framework (linq to objects, ef6, ef-core, etc.), because it simply generates an expression tree for the `IQueryable` `Select` method.

### Scoping and Chaining
The target projection can be obtained through (multiple, chained) internal query tranformations without changing the type of the original `IQueryable<T>`:

	protected IQueryable<Book> ApplyFilter(IQueryable<Book> query, BookFilter parameters, SelectOptionsContext<Book> filterContext)
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

		// replace Title and Description fields with localized versions from BooksLocalization (demonstrated in the sample project)
		if (CultureInfo.CurrentUICulture.Name != DefaultLanguage)
		{
			filterContext.UseScoped<BookLocalizationJoin>((options, q0) =>
			{
				var joinedQuery = q0.Join(Context.GetBooksLocalizationQuery(),
					b => b.Id,
					l => l.BookId,
					(b, l) => new BookLocalizationJoin { Book = b, Localization = l });
				options.Include(book => book.Title, it => it.Localization.Title);
				options.Include(book => book.Description, it => it.Localization.Description);
				return joinedQuery;
			}, it => it.Book);
		}

		// ...

		query = query.ApplySelectOptions(filterContext);

		return query;
	}