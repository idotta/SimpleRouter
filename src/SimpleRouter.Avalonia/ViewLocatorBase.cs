using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace SimpleRouter.Avalonia;

/// <summary>
/// Base class for view locators that implement the IDataTemplate interface.
/// </summary>
public abstract class ViewLocatorBase : IDataTemplate
{
    /// <summary>
    /// Gets or sets the default content to be used when no specific control is resolved.
    /// </summary>
    public Control? DefaultContent { get; set; }

    /// <summary>
    /// Builds the control based on the provided parameter.
    /// If the parameter is null or can't be resolved, the default content is returned.
    /// </summary>
    /// <param name="param">The parameter used to build the control.</param>
    /// <returns>The built control.</returns>
    public virtual Control? Build(object? param)
    {
        if (param == null)
        {
            return DefaultContent;
        }
        var control = ResolveControl(param as IRoute);
        if (control != null)
        {
            return control;
        }
        control = TryDeduceControl(param);
        if (control != null)
        {
            return control;
        }
        return DefaultContent;
    }

    /// <summary>
    /// Determines if the provided data matches the route.
    /// You can override this method to provide custom matching logic.
    /// </summary>
    /// <param name="data">The data to be matched.</param>
    /// <returns>True if the data matches the route; otherwise, false.</returns>
    public virtual bool Match(object? data)
    {
        return data is IRoute;
    }

    /// <summary>
    /// Resolves the control based on the provided route.
    /// It is highly recommended that you implement this method by matching each view to its
    /// viewmodel instead of using <see cref="TryDeduceControl"/>.
    /// If you still don't want to implement it, you can return null directly.
    /// </summary>
    /// <param name="route">The route used to resolve the control.</param>
    /// <returns>The resolved control.</returns>
    protected abstract Control? ResolveControl(IRoute? route);

    /// <summary>
    /// Tries to deduce the control based on the provided data by using reflection.
    /// </summary>
    /// <param name="data">The data used to deduce the control.</param>
    /// <returns>The deduced control.</returns>
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