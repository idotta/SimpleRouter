using Avalonia.Controls;
using SimpleRouter.Avalonia.Demo.ViewModels;

namespace SimpleRouter.Avalonia.Demo.Views
{
    [RouteView(typeof(Page3ViewModel), Group = "Nested")]
    public partial class Page3View : UserControl
    {
        public Page3View()
        {
            InitializeComponent();
        }
    }
}
