using System;
using Microsoft.AspNetCore.Components;

namespace Radzen.Blazor
{
    /// <summary>
    /// Registers and manages the current theme. Requires <see cref="ThemeService" /> to be registered in the DI container.
    /// </summary>
    public partial class RadzenTheme : IDisposable
    {
        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        [Parameter]
        public string Theme { get; set; }

        /// <summary>
        /// Enables WCAG contrast requirements. If set to true additional CSS file will be loaded.
        /// </summary>
        [Parameter]
        public bool WCAG { get; set; }

        /// <summary>
        /// Specifies if the theme should be right-to-left.
        /// </summary>
        [Parameter]
        public bool RightToLeft { get; set; }

        private string theme;

        private bool wcag;
        private bool rightToLeft;

        private string Href => $"{Path}/{theme}-base.css";

        private string WcagHref => $"{Path}/{theme}-wcag.css";

        private string Path => Premium ? $"css" : $"_content/Radzen.Blazor/css";

        private bool Premium => theme switch
        {
            "material3" => true,
            "fluent" => true,
            "fluent-dark" => true,
            "material3-dark" => true,
            _ => false
        };

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            theme = ThemeService.Theme ?? Theme;
            wcag = ThemeService.Wcag ?? WCAG;
            rightToLeft = ThemeService.RightToLeft ?? RightToLeft;

            ThemeService.SetTheme(theme, false);

            theme = theme.ToLowerInvariant();

            ThemeService.ThemeChanged += OnThemeChanged;

            base.OnInitialized();
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            theme = ThemeService.Theme.ToLowerInvariant();
            wcag = ThemeService.Wcag ?? WCAG;
            rightToLeft = ThemeService.RightToLeft ?? RightToLeft;

            StateHasChanged();
        }

        /// <summary>
        /// Releases all resources used by the component.
        /// </summary>
        public void Dispose()
        {
            ThemeService.ThemeChanged -= OnThemeChanged;
        }
    }
}