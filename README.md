# SimpleRouter

SimpleRouter is a lightweight, routing library for .NET applications. It was inspired by [ReactiveUI](https://github.com/reactiveui/ReactiveUI) routing and provides a simple and intuitive API for managing navigation within your application.

## Features

- Navigate to different routes in your application.
- Navigate back to the previous route.
- Navigate to a new route and reset the navigation stack.
- Use custom route factories to create routes.

## Getting Started

To use SimpleRouter in your project, you'll need to create classes that implement the `IRoute` and `IRouterHost` interfaces. `IRoute` represents a route in your application, and `IRouterHost` represents the host that the router operates within.

It works pretty well with MVVM Community Toolkit, but the design is framework agnostic.

Examples can be found in the Demo projects.

## Installation

You can install SimpleRouter via [NuGet Package](https://www.nuget.org/packages/IDotta.SimpleRouter/).

## Running the Tests

SimpleRouter comes with a suite of unit tests. To run the tests, navigate to the `SimpleRouter.Tests` directory and run the following command:
```
dotnet test
```

## Contributing

We welcome contributions to SimpleRouter! Please submit a pull request with your changes.

## License

SimpleRouter is licensed under the MIT License. See the `LICENSE` file for more information.