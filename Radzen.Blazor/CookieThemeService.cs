using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace Radzen
{
    /// <summary>
    /// Options for the <see cref="CookieThemeService" />.
    /// </summary>
    public class CookieThemeServiceOptions
    {
        /// <summary>
        /// Gets or sets the cookie name.
        /// </summary>
        public string Name { get; set; } = "Theme";

        /// <summary>
        /// Gets or sets the cookie duration.
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.FromDays(365);
    }

    /// <summary>
    /// Persist the current theme in a cookie. Requires <see cref="ThemeService" /> to be registered in the DI container.
    /// </summary>
    public class CookieThemeService
    {
        private readonly CookieThemeServiceOptions options;
        private readonly IJSRuntime jsRuntime;
        private readonly ThemeService themeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CookieThemeService" /> class.
        /// </summary>
        public CookieThemeService(IJSRuntime jsRuntime, ThemeService themeService, CookieThemeServiceOptions options = null)
        {
            this.jsRuntime = jsRuntime;
            this.themeService = themeService;
            this.options = options ?? new CookieThemeServiceOptions();

            themeService.ThemeChanged += OnThemeChanged;

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                var cookies = await jsRuntime.InvokeAsync<string>("eval", "document.cookie");

                var themeCookie = cookies?.Split("; ").Select(x =>
                {
                    var parts = x.Split("=");

                    return (Key: parts[0], Value: parts[1]);
                })
                .FirstOrDefault(x => x.Key == "Theme");

                var theme = themeCookie?.Value;

                if (!string.IsNullOrEmpty(theme) && themeService.Theme != theme)
                {
                    themeService.SetTheme(theme);
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            var expiration = DateTime.Now.Add(options.Duration);

            _ = jsRuntime.InvokeVoidAsync("eval", $"document.cookie = \"{options.Name}={themeService.Theme}; expires={expiration:R}; path=/\"");
        }
    }

    /// <summary>
    /// Extension methods to register the <see cref="CookieThemeService" />.
    /// </summary>
    public static class CookieThemeServiceExtensions
    {
        /// <summary>
        /// Adds the <see cref="CookieThemeService" /> to the service collection.
        /// </summary>
        public static void AddRadzenQueryStringThemeService(this IServiceCollection services)
        {
            services.AddScoped<CookieThemeService>();
        }

        /// <summary>
        /// Adds the <see cref="CookieThemeService" /> to the service collection with the specified options.
        /// </summary>
        public static void AddRadzenCookieThemeService(this IServiceCollection services, CookieThemeServiceOptions options)
        {
            services.AddScoped(serviceProvider =>
            {
                var themeService = serviceProvider.GetRequiredService<ThemeService>();
                var jsRuntime = serviceProvider.GetRequiredService<IJSRuntime>();
                return new CookieThemeService(jsRuntime, themeService, options ?? new CookieThemeServiceOptions());
            });
        }
    }
}