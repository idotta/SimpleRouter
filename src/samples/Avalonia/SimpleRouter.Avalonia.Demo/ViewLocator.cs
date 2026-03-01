using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SimpleRouter.Avalonia.Demo.ViewModels;

namespace SimpleRouter.Avalonia.Demo;

[GeneratedViewLocator(Group = "Main")]
public partial class ViewLocator : ViewLocatorBase
{
    public override bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}

[GeneratedViewLocator(Group = "Nested")]
public partial class NestedViewLocator : ViewLocatorBase
{
    public NestedViewLocator()
    {
        DefaultContent = new TextBlock { Text = "Nested default content", TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
    }

    public override bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
