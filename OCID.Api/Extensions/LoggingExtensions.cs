using NLog.Web;

namespace OCID.Api.Extensions;

public static class LoggingExtensions
{
    /// <summary>
    /// Replaces the default logging providers with NLog, configured by <c>nlog.config</c>.
    /// </summary>
    /// <param name="builder">Application builder to configure.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static WebApplicationBuilder AddNLogLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();
        return builder;
    }
}
