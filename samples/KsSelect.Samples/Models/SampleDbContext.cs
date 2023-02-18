using KsSelect.Samples.Models;
using Microsoft.EntityFrameworkCore;

namespace KsSelect.Samples.Infrastructure;

public interface ISampleDbContext
{
	DbSet<T> Set<T>() where T : class;

	IQueryable<Author> GetAuthorsQuery();

	IQueryable<Book> GetBooksQuery();
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

		base.OnModelCreating(modelBuilder);
	}

	DbSet<T> ISampleDbContext.Set<T>() => base.Set<T>();

	public DbSet<Author> Authors { get; set; }

	public DbSet<Book> Books { get; set; }

	public IQueryable<Author> GetAuthorsQuery() => Authors;

	public IQueryable<Book> GetBooksQuery() => Books;
}
