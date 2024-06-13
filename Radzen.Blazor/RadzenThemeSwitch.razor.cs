using System;
using Microsoft.AspNetCore.Components;

namespace Radzen.Blazor
{
    /// <summary>
    /// Dark or light theme switch. Requires <see cref="ThemeService" /> to be registered in the DI container.
    /// </summary>
    public partial class RadzenThemeSwitch : RadzenComponent
    {
///     <summary>
        /// Gets or sets the switch button variant.
        /// </summary>
        /// <value>The switch button variant.</value>
        [Parameter]
        public Variant Variant { get; set; } = Variant.Text;

        /// <summary>
        /// Gets or sets the switch button style.
        /// </summary>
        /// <value>The switch button style.</value>
        [Parameter]
        public ButtonStyle ButtonStyle { get; set; } = ButtonStyle.Base;

        /// <summary>
        /// Gets or sets the switch button toggled shade.
        /// </summary>
        /// <value>The switch button toggled shade.</value>
        [Parameter]
        public Shade ToggleShade { get; set; } = Shade.Default;

        /// <summary>
        /// Gets or sets the switch button toggled style.
        /// </summary>
        /// <value>The switch button toggled style.</value>
        [Parameter]
        public ButtonStyle ToggleButtonStyle { get; set; } = ButtonStyle.Base;

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

        private string Icon => value ? "dark_mode" : "light_mode";

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();

            ThemeService.ThemeChanged -= OnThemeChanged;
        }
    }
}