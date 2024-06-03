# SimpleRouter

![.NET Tests](https://github.com/idotta/SimpleRouter/actions/workflows/dotnet-test.yml/badge.svg)

<div align="center">
<br>
<a href="https://github.com/idotta/SimpleRouter">
  <img width="160" heigth="160" src="./images/logo.png">
</a>
<br>
</div>

SimpleRouter is a lightweight and flexible routing library for .NET applications. It was inspired by [ReactiveUI](https://github.com/reactiveui/ReactiveUI) routing and provides a simple and intuitive API for managing navigation within your application.

## Features

- Navigate to different routes in your application.
- Navigate back to the previous route.
- Navigate to a new route and reset the navigation stack.
- Use custom route factories to create routes.

## Getting Started

To use SimpleRouter in your project, you'll need to create classes that implement the `IRoute` and `IRouterHost` interfaces. `IRoute` represents a route in your application, and `IRouterHost` represents the host that the router operates within.

It works pretty well with MVVM Community Toolkit, but the design is framework agnostic.

Examples can be found in the samples directory.

More advanced features are available in the SimpleRouter.Avalonia extension, which provides additional features for managing views in your Avalonia application. These can be replicated in a WPF application, for example.

## Installation

You can install SimpleRouter via [NuGet Package](https://www.nuget.org/packages/IDotta.SimpleRouter/).

## SimpleRouter.Avalonia

SimpleRouter.Avalonia is an extension of SimpleRouter for Avalonia applications. It provides additional features for managing views in your Avalonia application.

### Features

- **RouteViewHost**: A `TransitioningContentControl` that hosts the current route's view. It listens to route changes in the `Router` and updates the displayed content accordingly. It also supports a `DefaultContent` property that is displayed when there is no current route.

- **ViewLocatorBase**: An abstract base class for view locators that implement the `IDataTemplate` interface. It provides methods for building and matching controls based on routes. It also supports a `DefaultContent` property that is used when no specific control is resolved.

### Usage

To use SimpleRouter.Avalonia in your project, you'll need to create a class that extends `ViewLocatorBase` and implement the `ResolveControl` method. This method should return the appropriate control for each route in your application.

You'll also need to add a `RouteViewHost` to your Avalonia UI and bind its `Router` property to your `Router` instance. The `RouteViewHost` will automatically update its content to match the current route.

You must create at least one ViewLocator for your application. The ViewLocator is responsible for resolving views based on routes. You can register it as a DataTemplate in App.axaml, like:

```xml
<Application.DataTemplates>
    <local:ViewLocator />
</Application.DataTemplates>
```

You can also create different ViewLocators for different parts of your application. For example, you might have a ViewLocator for your main content and another for nested components, injecting the second directly to the RouteViewHost.

Here's an example of how you might set up a `RouteViewHost` in your Avalonia UI:

```
xmlns:simplerouter="clr-namespace:SimpleRouter.Avalonia;assembly=SimpleRouter.Avalonia"

...

<UserControl.Resources>
    <root:ViewLocator x:Key="viewLocator" />
</UserControl.Resources>

...

<simplerouter:RouteViewHost Router="{Binding Router}" ViewLocator="{StaticResource viewLocator}" />
```
In this example, `Router` is a property in your view model and `ViewLocator` is declared in your xaml file. This control can be nested inside another RouteViewHost, allowing you to create complex navigation structures.

### Installation

You can install SimpleRouter.Avalonia via [NuGet Package](https://www.nuget.org/packages/IDotta.SimpleRouter.Avalonia/).

## Running the Tests

SimpleRouter comes with a suite of unit tests. To run the tests, navigate to the `SimpleRouter.Tests` directory and run the following command:
```
dotnet test
```

## Contributing

We welcome contributions to SimpleRouter! Please submit a pull request with your changes.

## License

SimpleRouter is licensed under the MIT License. See the `LICENSE` file for more information.