using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SimpleRouter.Avalonia.Demo.ViewModels;
using SimpleRouter.Avalonia.Demo.Views;
using System;

namespace SimpleRouter.Avalonia.Demo;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return data switch
        {
            Page1ViewModel p1 => new Page1View { DataContext = p1 },
            Page2ViewModel p2 => new Page2View { DataContext = p2 },
            _ => TryDeduceControl(data),
        };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    private static Control? TryDeduceControl(object data)
    {
        var name = data.GetType().FullName?.Replace("ViewModel", "View") ?? "";
        var type = Type.GetType(name);
        if (type != null)
        {
            return Activator.CreateInstance(type) as Control;
        }
        return null;
    }
}