# select-options (aka ks-select)
[![NuGet](https://img.shields.io/nuget/v/Kapusons.Select?label=NuGet)](https://www.nuget.org/packages/Kapusons.Select)

Augments LINQ `IQueryable` `Select` capabilities with configurable options to allow build composable projections, based on conditional constructs.
> works virtually with any LINQ provider and LINQ-based framework (LINQ to objects, ef6, ef-core, etc.), because it simply generates an expression tree for the `IQueryable` `Select` method.

---
```csharp
	query.Select<Product, ProductInfo>(options =>
	{
		options.Include(it => it.Category, it => categoryQuery.Where(c => c.Id == it.CategoryId).Select(c => c.Name).FirstOrDefault());
		options.Exclude(it => it.Description);
	});
```

## Features

### Conditionally exclude or include columns
```csharp
	if (excludeBinaryData) query = query.Select(options => { options.Exclude(it => it.BinaryData); });
	if (includeOrderCount) query = query.Select(options => { options.Include(it.OrderCount, it => orderQuery.Count(o => o.ProductId == it.Id)) });
```
or
```csharp
	query = query.Select(options =>
	{
		if (excludeBinaryData) options.Exclude(it => it.BinaryData);
		if (includeOrderCount) options.Include(it.OrderCount, it => orderQuery.Count(o => o.ProductId == it.Id));
	});
```

> The extra columns can be applied either to the original entity (usually those columns are not mapped to the data store and are marked with the `[NotMapped]` attribute)
> or to a distinct type (see the demo project for an example)

### AutoMapper-like projection
Properties with a matching name are automatically mapped from the original type to the target type. Supports complex type properties.

### Provider agnostic
Works virtually with any LINQ provider and LINQ-based framework (LINQ to objects, ef6, ef-core, etc.), because it simply generates an expression tree for the `IQueryable` `Select` method.

### Scoping and Chaining
The target projection can be obtained through (multiple, chained) internal query transformations without changing the type of the original `IQueryable<T>`:
```csharp
	protected IQueryable<Book> ApplyFilter(IQueryable<Book> query, BookFilter parameters, SelectOptionsContext<Book> filterContext)
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
```

### No collisions/ambiguities with standard Queryable Select methods
The different query overloads can be both used in the same context without explicit casts/type parameters, because the compiler (and Visual Studio Intellisense) is smart enough to pick automatically the right method:

```csharp
// standard Queryable methods
query.Select(it => it.Name);
query.Select(it => new { it.Name });

// select-options method
query.Select(o => o.Include(it => it.RelatedItemCount, it => relatedItemQuery.Where(r => r.RelatedId == it.Id).Count()));
```
