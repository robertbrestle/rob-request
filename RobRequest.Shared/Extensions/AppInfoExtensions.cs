using System.Reflection;

namespace RobRequest.Shared.Extensions;

public static class AppInfoExtensions
{
    public static string GetAppVersion =>
        typeof(AppInfoExtensions).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion?.Split('+')[0]
        ?? typeof(AppInfoExtensions).Assembly.GetName().Version?.ToString(3)
        ?? "0.0.0";
}