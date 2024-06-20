using System.Reflection;

namespace Rockaway.WebApp.Services
{
    public class StatusReporter : IStatusReporter
    {
        private static readonly Assembly assembly = Assembly.GetEntryAssembly()!;
        private readonly DateTime startTime;

        public StatusReporter() : this(DateTime.UtcNow) { }

        // Allow injection of start time for testing
        public StatusReporter(DateTime startTime)
        {
            this.startTime = startTime;
        }

        public ServerStatus GetStatus() => new()
        {
            Assembly = assembly.FullName ?? "Assembly.GetEntryAssembly() returned null",
            Modified = new DateTimeOffset(File.GetLastWriteTimeUtc(assembly.Location), TimeSpan.Zero).ToString("O"),
            Hostname = Environment.MachineName,
            DateTime = DateTimeOffset.UtcNow.ToString("O"),
            Uptime = GetUptime()
        };

        private string GetUptime()
        {
            var uptime = DateTime.UtcNow - startTime;
            return $"{(int)uptime.TotalHours}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
        }

        public int GetUptimeInSeconds()
        {
            var uptime = DateTime.UtcNow - startTime;
            return (int)uptime.TotalSeconds;
        }
    }
}