using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Linq;
using VaM.ModManager.App.Enums;

namespace VaM.ModManager.App.Views
{
    public class AddVmmView : UserControl
    {
        public AddVmmView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            var cboModType = this.FindControl<ComboBox>("cboModType");
            cboModType.Items = Enum.GetValues(typeof(ModTypeEnum)).Cast<ModTypeEnum>();
        }
    }
}
