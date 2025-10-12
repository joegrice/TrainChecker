using System.Reflection;

namespace TrainChecker.Helpers;

public static class ApplicationVersion
{
    public static readonly string Name = Assembly.GetEntryAssembly()?.GetName().Name;
    public static readonly string Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
}