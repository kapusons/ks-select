using KsSelect.Samples.Models;
using Microsoft.EntityFrameworkCore;

namespace KsSelect.Samples.Infrastructure;

public interface ISampleDbContext
{
	DbSet<T> Set<T>() where T : class;

	IQueryable<Author> GetAuthorsQuery();

	IQueryable<Book> GetBooksQuery();

	IQueryable<BookLocalization> GetBooksLocalizationQuery();
}

public class SampleDbContext : DbContext, ISampleDbContext
{
	public SampleDbContext(DbContextOptions options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Author>().HasData(
			new Author() { Id = 1, FirstName = "Sthepen", LastName = "King" },
			new Author { Id = 2, FirstName = "Neil ", LastName = "Gaiman" });

		modelBuilder.Entity<Book>().HasData(
			new Book { Id = 1, AuthorId = 1, Title = "IT", Description = "lorem ipsum dolor" },
			new Book { Id = 2, AuthorId = 1, Title = "Misery", Description = "lorem ipsum dolor" },
			new Book { Id = 3, AuthorId = 1, Title = "Cujo", Description = "lorem ipsum dolor" },
			new Book { Id = 4, AuthorId = 2, Title = "American Gods", Description = "lorem ipsum dolor" });

		modelBuilder.Entity<BookLocalization>().HasData(
			new BookLocalization { Id = 1, BookId = 1, Language = "IT", Title = "Titolo italiano libro 1", Description = "descrizione in italiano libro 1" },
			new BookLocalization { Id = 2, BookId = 2, Language = "IT", Title = "Titolo italiano libro 2", Description = "descrizione in italiano libro 2" },
			new BookLocalization { Id = 3, BookId = 3, Language = "IT", Title = "Titolo italiano libro 3", Description = "descrizione in italiano libro 3" },
			new BookLocalization { Id = 4, BookId = 4, Language = "IT", Title = "Titolo italiano libro 4", Description = "descrizione in italiano libro 4" });

		base.OnModelCreating(modelBuilder);
	}

	DbSet<T> ISampleDbContext.Set<T>() => base.Set<T>();

	private DbSet<Author> Authors { get; set; }

	private DbSet<Book> Books { get; set; }

	private DbSet<BookLocalization> BooksLocalization { get; set; }

	public IQueryable<Author> GetAuthorsQuery() => Authors;

	public IQueryable<Book> GetBooksQuery() => Books;

	public IQueryable<BookLocalization> GetBooksLocalizationQuery() => BooksLocalization;
}
