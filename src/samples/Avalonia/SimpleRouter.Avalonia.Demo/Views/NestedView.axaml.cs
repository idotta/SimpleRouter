using Avalonia.Controls;
using SimpleRouter.Avalonia.Demo.ViewModels;

namespace SimpleRouter.Avalonia.Demo.Views
{
    [RouteView(typeof(NestedViewModel), Group = "Main")]
    public partial class NestedView : UserControl
    {
        public NestedView()
        {
            InitializeComponent();
        }
    }
}
