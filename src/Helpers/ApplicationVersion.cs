using System.Reflection;

namespace TrainChecker.Helpers;

public static class ApplicationVersion
{
    public static readonly string Name = Assembly.GetEntryAssembly()?.GetName().Name;
    public static readonly string Version = Environment.GetEnvironmentVariable("APP_VERSION") ?? "unknown";
}