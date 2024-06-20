using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Rockaway.WebApp.Services;
using Shouldly;

namespace Rockaway.WebApp.Tests {
	public class EndpointTests {

		[Fact]
		public async Task Status_Endpoint_Works() {
			await using var factory = new WebApplicationFactory<Program>();
			var client = factory.CreateClient();

			var result = await client.GetAsync("/status");
			
			result.EnsureSuccessStatusCode();
		}
		
		private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

		private static readonly ServerStatus testStatus = new() {
			Assembly = "TEST_ASSEMBLY",
			Modified = new DateTimeOffset(2021, 2, 3, 4, 5, 6, TimeSpan.Zero).ToString("O"),
			Hostname = "TEST_HOST",
			DateTime = new DateTimeOffset(2022, 3, 4, 5, 6, 7, TimeSpan.Zero).ToString("O"),
			Uptime = "0:00:00"  // Adjust as needed for testing
		};

		private class TestStatusReporter : IStatusReporter {
			public ServerStatus GetStatus() => testStatus;
		}
		[Fact]
		public async Task Status_Endpoint_Returns_Status() {
			await using var factory = new WebApplicationFactory<Program>()
				.WithWebHostBuilder(builder => builder.ConfigureServices(services => {
					services.AddSingleton<IStatusReporter>(new TestStatusReporter());
				}));
			using var client = factory.CreateClient();
			var json = await client.GetStringAsync("/status");
			Console.WriteLine("Response from /status endpoint: " + json);  // Output the response to the console
			var status = JsonSerializer.Deserialize<ServerStatus>(json, jsonSerializerOptions);
			status.ShouldNotBeNull();
			status.ShouldBeEquivalentTo(testStatus);
		}
		[Fact]
        public async Task Uptime_Endpoint_Returns_Uptime()
        {
            var fakeStartTime = DateTime.UtcNow.AddSeconds(-120); // Mock 2 minutes of uptime
            await using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder => builder.ConfigureServices(services => {
                    services.AddSingleton<IStatusReporter>(new StatusReporter(fakeStartTime));
                }));
            using var client = factory.CreateClient();
            var result = await client.GetStringAsync("/uptime");
            var uptimeInSeconds = int.Parse(result);
			Console.WriteLine("uptime: " + result);  
            uptimeInSeconds.ShouldBeGreaterThan(0);  // Verify uptime is greater than 0 seconds
            uptimeInSeconds.ShouldBeGreaterThan(119);  // Verify uptime is greater than 119 seconds
        }


		[Fact]
        public async Task Uptime_Endpoint_Returns_Uptime_In_Seconds()
        {
            var fakeStartTime = DateTime.UtcNow.AddSeconds(-120); // Mock 2 minutes of uptime
            await using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder => builder.ConfigureServices(services => {
                    services.AddSingleton<IStatusReporter>(new StatusReporter(fakeStartTime));
                }));
            using var client = factory.CreateClient();
            var result = await client.GetStringAsync("/uptime");
            var uptimeInSeconds = int.Parse(result);
            uptimeInSeconds.ShouldBeGreaterThan(0);  // Verify uptime is greater than 0 seconds
            uptimeInSeconds.ShouldBeGreaterThan(119);  // Verify uptime is greater than 119 seconds
        }
	}
}