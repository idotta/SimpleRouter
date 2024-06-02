using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using SimpleRouter.Avalonia.Demo.ViewModels;
using SimpleRouter.Avalonia.Demo.Views;
using System;

namespace SimpleRouter.Avalonia.Demo;

public class ViewLocator : ViewLocatorBase
{
    public override bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    protected override Control? ResolveControl(IRoute? route)
    {
        ArgumentNullException.ThrowIfNull(route);
        return route switch
        {
            Page1ViewModel p1 => new Page1View { DataContext = p1 },
            Page2ViewModel p2 => new Page2View { DataContext = p2 },
            NestedViewModel r => new NestedView { DataContext = r },
            _ => DefaultContent
        };
    }
}

public class NestedViewLocator : ViewLocatorBase
{
    public NestedViewLocator()
    {
        DefaultContent = new TextBlock { Text = "Nested default content", TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center  };
    }

    public override bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    protected override Control? ResolveControl(IRoute? route)
    {
        ArgumentNullException.ThrowIfNull(route);
        return route switch
        {
            Page3ViewModel p3 => new Page3View { DataContext = p3 },
            _ => DefaultContent
        };
    }
}