using KsSelect.Samples.Infrastructure;
using KsSelect.Samples.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddKsSelectContext(this IServiceCollection services, IConfiguration configuration)
	{
		if (services is null) throw new ArgumentNullException(nameof(services));
		if (configuration is null) throw new ArgumentNullException(nameof(configuration));

		var connectionString = configuration.GetConnectionString("Data");
		services.AddDbContext<SampleDbContext>(o => o.UseSqlServer(connectionString));
		services.AddScoped<ISampleDbContext, SampleDbContext>();

		return services;
	}

	public static IServiceCollection AddRepositories(this IServiceCollection services)
	{
		if (services is null) throw new ArgumentNullException(nameof(services));

		return services.Scan(scan => scan.FromAssemblyOf<BookRepository>()
			.AddClasses(classes => classes.AssignableTo<IRepository>())
			.AsImplementedInterfaces()
			.WithScopedLifetime());
	}
}
