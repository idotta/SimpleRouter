using Avalonia.Controls;
using SimpleRouter.Avalonia.Demo.ViewModels;

namespace SimpleRouter.Avalonia.Demo.Views
{
    [RouteView(typeof(Page2ViewModel), Group = "Main")]
    public partial class Page2View : UserControl
    {
        public Page2View()
        {
            InitializeComponent();
        }
    }
}
