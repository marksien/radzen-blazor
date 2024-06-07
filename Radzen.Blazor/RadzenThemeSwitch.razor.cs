using System;
using Microsoft.AspNetCore.Components;

namespace Radzen.Blazor
{
    /// <summary>
    /// Dark or light theme switch. Requires <see cref="ThemeService" /> to be registered in the DI container.
    /// </summary>
    public partial class RadzenThemeSwitch : RadzenComponent
    {
        /// <summary>
        /// Gets or sets the light theme. Set to <c>Default</c> by default.
        /// </summary>
        [Parameter]
        public string LightTheme { get; set; } = "Default";

        /// <summary>
        /// Gets or sets the dark theme. Set to <c>Dark</c> by default.
        /// </summary>
        [Parameter]
        public string DarkTheme { get; set; } = "Dark";

        private bool value;

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            base.OnInitialized();

            ThemeService.ThemeChanged += OnThemeChanged;

            value = ThemeService.Theme != DarkTheme;
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            value = ThemeService.Theme != DarkTheme;

            StateHasChanged();
        }

        void OnChange(bool value)
        {
            ThemeService.SetTheme(value ? LightTheme : DarkTheme);
        }

        private string Icon => value ? "wb_sunny" : "nights_stay";

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();

            ThemeService.ThemeChanged -= OnThemeChanged;
        }
    }
}