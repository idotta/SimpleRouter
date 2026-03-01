using Avalonia.Controls;
using SimpleRouter.Avalonia.Demo.ViewModels;

namespace SimpleRouter.Avalonia.Demo.Views
{
    [RouteView(typeof(Page1ViewModel), Group = "Main")]
    public partial class Page1View : UserControl
    {
        public Page1View()
        {
            InitializeComponent();
        }
    }
}
