using DotNetBrightener.DataAccess.EF.PostgreSQL.Tests.TemporalTable.TestEntities;
using DotNetBrightener.DataAccess.EF.PostgreSQL.Extensions;
using DotNetBrightener.DataAccess.EF.PostgreSQL.Interceptors;
using DotNetBrightener.TestHelpers;
using DotNetBrightener.TestHelpers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using Xunit.Abstractions;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.Tests.TemporalTable;

/// <summary>
/// 	Base test class for temporal table tests
/// </summary>
public abstract class TemporalTestBase : PostgreSqlServerBaseXUnitTest
{
	private readonly ITestOutputHelper _testOutputHelper;
	private readonly List<IHost> _hosts = new();

	/// <summary>
	/// 	Initializes a new instance of the <see cref="TemporalTestBase"/> class
	/// </summary>
	protected TemporalTestBase(ITestOutputHelper testOutputHelper)
		: base(testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
	}

	/// <summary>
	/// 	Creates a test host following the project pattern
	/// </summary>
	protected IHost CreateTestHost(Action<IServiceCollection>? configureServices = null)
	{
		var host = XUnitTestHost.CreateTestHost(_testOutputHelper,
			(hostContext, services) =>
			{
				// Register PostgreSQL history services
				services.AddPostgreSqlHistoryServices();

				services.AddDbContext<TemporalTestDbContext>((provider, options) =>
				{
					options.UseNpgsql(ConnectionString);
					options.EnableSensitiveDataLogging();

					// Add the history interceptor from DI
					var interceptor = provider.GetRequiredService<PostgreSqlHistoryInterceptor>();
					options.AddInterceptors(interceptor);
				});

				configureServices?.Invoke(services);
			});

		_hosts.Add(host);
		return host;
	}

	/// <summary>
	/// 	Dispose resources
	/// </summary>
	public void Dispose()
	{
		foreach (var host in _hosts)
		{
			host.Dispose();
		}
	}
}
