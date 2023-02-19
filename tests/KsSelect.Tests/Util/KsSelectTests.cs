using Kapusons.Components.Util;

namespace Kapusons.Components.Util.Tests
{
	public class KsSelectTests
	{
		private class Product
		{
			public int CategoryId { get; set; }

			public string? Name { get; set; }

			public string? Description { get; set; }
		}
		private class ProductInfo
		{
			public string? Name { get; set; }

			public string? Category { get; set; }

			public string? Description { get; set; }
		}

		public class Category
		{
			public int Id { get; set; }

			public string? Name { get; set; }
		}

		[Fact]
		public void SelectOptionsTest()
		{
			// Arrange
			var categoryQuery = new[]
			{
				new Category { Id = 1, Name = "Category 1" },
				new Category { Id = 2, Name = "Category 2" },
			};
			var query = new[]
			{
				new Product { Name = "Product 1", CategoryId = 1, Description = "long description" },
				new Product { Name = "Product 2", CategoryId = 1 },
				new Product { Name = "Product 3", CategoryId = 2 },
			}.AsQueryable();

			// Act
			//query = query.Select(options => { });
			var result = query.Select<Product, ProductInfo>(options =>
			{
				options.Include(it => it.Category, it => categoryQuery.Where(c => c.Id == it.CategoryId).Select(c => c.Name).FirstOrDefault());
				options.Exclude(it => it.Description);
			}).ToList();

			// Assert
			Assert.NotEmpty(result);
			Assert.Contains(result, it => it.Name == "Product 1" && it.Category == "Category 1");
			Assert.Contains(result, it => it.Name == "Product 2" && it.Category == "Category 1");
			Assert.Contains(result, it => it.Name == "Product 3" && it.Category == "Category 2");
			Assert.All(result, it => Assert.Null(it.Description));
		}
	}
}