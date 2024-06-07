using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

#if NET7_0_OR_GREATER
namespace Radzen
{
    /// <summary>
    /// Options for the <see cref="QueryStringThemeService" />.
    /// </summary>
    public class QueryStringThemeServiceOptions
    {
        /// <summary>
        /// Gets or sets the query string parameter for the theme.
        /// </summary>
        public string ThemeParameter { get; set; } = "theme";
        /// <summary>
        /// Gets or sets the query string parameter for the wcag compatible color theme.
        /// </summary>
        public string WcagParameter { get; set; } = "wcag";

        /// <summary>
        /// Gets or sets the query string parameter for the right to left theme.
        /// </summary>
        public string RightToLeftParameter { get; set; } = "rtl";
    }

    /// <summary>
    /// Persist the current theme in the query string. Requires <see cref="ThemeService" /> to be registered in the DI container.
    /// </summary>
    public class QueryStringThemeService : IDisposable
    {
        private NavigationManager NavigationManager { get; }
        private ThemeService ThemeService { get; }
        private readonly IDisposable registration;

        /// <summary>
        /// Gets or sets the options for the <see cref="QueryStringThemeService" />.
        /// </summary>
        public QueryStringThemeServiceOptions Options { get; set; } = new ();

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStringThemeService" /> class.
        /// </summary>
        public QueryStringThemeService(NavigationManager navigationManager, ThemeService themeService)
        {
            NavigationManager = navigationManager;

            ThemeService = themeService;

            var state = GetStateFromQueryString(NavigationManager.Uri);

            if (state.theme != null && RequiresChange(state))
            {
                ThemeService.SetTheme(new ThemeOptions
                {
                    Theme = state.theme,
                    Wcag = state.wcag,
                    RightToLeft = state.rightToLeft,
                    TriggerChange = true
                });
            }

            ThemeService.ThemeChanged += OnThemeChanged;

            try
            {
                registration = NavigationManager.RegisterLocationChangingHandler(OnLocationChanging);
            }
            catch (NotSupportedException)
            {
                // HttpNavigationManager does not support that
            }
        }

        private bool RequiresChange((string theme, bool? wcag, bool? rightToLeft) state) =>
            !string.Equals(ThemeService.Theme, state.theme, StringComparison.OrdinalIgnoreCase) ||
            ThemeService.Wcag != state.wcag || ThemeService.RightToLeft != state.rightToLeft;

        private ValueTask OnLocationChanging(LocationChangingContext context)
        {
            var state = GetStateFromQueryString(context.TargetLocation);

            if (RequiresChange(state))
            {
                context.PreventNavigation();

                NavigationManager.NavigateTo(GetUriWithStateQueryParameters(context.TargetLocation), replace: true);
            }

            return ValueTask.CompletedTask;
        }

        private (string theme, bool? wcag, bool? rightToLeft) GetStateFromQueryString(string uri)
        {
            var query = HttpUtility.ParseQueryString(new Uri(uri).Query);

            bool? wcag = query.Get(Options.WcagParameter) != null ? query.Get(Options.WcagParameter) == "true" : null;
            bool? rtl = query.Get(Options.RightToLeftParameter) != null ? query.Get(Options.RightToLeftParameter) == "true" : null;

            return (query.Get(Options.ThemeParameter), wcag, rtl);
        }

        private string GetUriWithStateQueryParameters(string uri)
        {
            var parameters = new Dictionary<string, object>
            {
                { Options.ThemeParameter, ThemeService.Theme.ToLowerInvariant() },
            };

            if (ThemeService.Wcag.HasValue)
            {
                parameters.Add(Options.WcagParameter, ThemeService.Wcag.Value ? "true" : "false");
            }

            if (ThemeService.RightToLeft.HasValue)
            {
                parameters.Add(Options.RightToLeftParameter, ThemeService.RightToLeft.Value ? "true" : "false");
            }

            return NavigationManager.GetUriWithQueryParameters(uri, parameters);
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            var state = GetStateFromQueryString(NavigationManager.Uri);

            NavigationManager.NavigateTo(GetUriWithStateQueryParameters(NavigationManager.Uri),
                forceLoad: state.rightToLeft != ThemeService.RightToLeft);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            ThemeService.ThemeChanged -= OnThemeChanged;
            registration?.Dispose();
        }
    }

    /// <summary>
    /// Extension methods to register the <see cref="QueryStringThemeService" />.
    /// </summary>
    public static class QueryStringThemeServiceExtensions
    {
        /// <summary>
        /// Adds the <see cref="QueryStringThemeService" /> to the service collection.
        /// </summary>
        public static void AddRadzenQueryStringThemeService(this IServiceCollection services)
        {
            services.AddScoped<QueryStringThemeService>();
        }

        /// <summary>
        /// Adds the <see cref="QueryStringThemeService" /> to the service collection with the specified options.
        /// </summary>
        public static void AddRadzenQueryStringThemeService(this IServiceCollection services, QueryStringThemeServiceOptions options)
        {
            services.AddScoped(serviceProvider =>
            {
                var navigationManager = serviceProvider.GetRequiredService<NavigationManager>();
                var themeService = serviceProvider.GetRequiredService<ThemeService>();
                var queryStringThemeService = new QueryStringThemeService(navigationManager, themeService);

                if (options != null)
                {
                    queryStringThemeService.Options = options;
                }

                return queryStringThemeService;
            });
        }
    }
}
#endif