using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SimpleRouter.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public sealed class RouteFactoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Emit attribute sources
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("RouteAttribute.g.cs", SourceText.From(AttributeSource.RouteAttribute, Encoding.UTF8));
            ctx.AddSource("GeneratedRouteFactoryAttribute.g.cs", SourceText.From(AttributeSource.GeneratedRouteFactoryAttribute, Encoding.UTF8));
        });

        // Collect [Route]-marked classes
        var routeClasses = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeSource.RouteAttributeName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, ct) => GetRouteInfo(ctx, ct))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!.Value);

        // Collect [GeneratedRouteFactory]-marked classes
        var factoryHosts = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeSource.GeneratedRouteFactoryAttributeName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => GetFactoryHostInfo(ctx))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!.Value);

        // Combine routes and hosts
        var combined = factoryHosts.Combine(routeClasses.Collect());

        context.RegisterSourceOutput(combined, static (spc, pair) =>
        {
            var (host, routes) = pair;

            // Report diagnostic if not partial
            if (!host.IsPartial)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.RouteFactoryClassMustBePartial,
                    host.Location,
                    host.ClassName));
                return;
            }

            // Report diagnostics for routes that don't implement IRoute
            foreach (var route in routes)
            {
                if (!route.ImplementsIRoute)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.RouteClassMustImplementIRoute,
                        route.Location,
                        route.ClassName));
                }
            }

            var hostGroup = host.Group;
            var validRoutes = routes.Where(r => r.ImplementsIRoute && r.Group == hostGroup).ToImmutableArray();
            GenerateFactory(spc, host, validRoutes);
        });
    }

    private static RouteInfo? GetRouteInfo(GeneratorAttributeSyntaxContext ctx, System.Threading.CancellationToken ct)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol)
            return null;

        var implementsIRoute = ImplementsInterface(typeSymbol, "SimpleRouter.IRoute");

        // Extract Group from the [Route] attribute's named arguments
        var attr = ctx.Attributes.FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString() == "SimpleRouter.RouteAttribute");
        var group = attr?.NamedArguments.FirstOrDefault(kvp => kvp.Key == "Group").Value.Value as string ?? "";

        // Find the best constructor: single public constructor, or the one with the most parameters
        var constructors = typeSymbol.Constructors
            .Where(c => !c.IsStatic && c.DeclaredAccessibility == Accessibility.Public)
            .OrderByDescending(c => c.Parameters.Length)
            .ToImmutableArray();

        var ctorParams = ImmutableArray<ConstructorParam>.Empty;
        if (constructors.Length > 0)
        {
            var ctor = constructors[0];
            ctorParams = ctor.Parameters
                .Select(p => new ConstructorParam(p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), p.Name))
                .ToImmutableArray();
        }

        return new RouteInfo(
            typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            typeSymbol.Name,
            implementsIRoute,
            ctorParams,
            group,
            ctx.TargetNode.GetLocation());
    }

    private static FactoryHostInfo? GetFactoryHostInfo(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol)
            return null;

        var classDecl = (ClassDeclarationSyntax)ctx.TargetNode;
        var isPartial = classDecl.Modifiers.Any(SyntaxKind.PartialKeyword);

        // Extract Group from the [GeneratedRouteFactory] attribute's named arguments
        var attr = ctx.Attributes.FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString() == "SimpleRouter.GeneratedRouteFactoryAttribute");
        var group = attr?.NamedArguments.FirstOrDefault(kvp => kvp.Key == "Group").Value.Value as string ?? "";

        var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : typeSymbol.ContainingNamespace.ToDisplayString();

        return new FactoryHostInfo(
            typeSymbol.Name,
            ns,
            isPartial,
            group,
            ctx.TargetNode.GetLocation());
    }

    private static void GenerateFactory(SourceProductionContext spc, FactoryHostInfo host, ImmutableArray<RouteInfo> routes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        if (host.Namespace is not null)
        {
            sb.AppendLine($"namespace {host.Namespace}");
            sb.AppendLine("{");
        }

        sb.AppendLine($"    partial class {host.ClassName}");
        sb.AppendLine("    {");

        // GeneratedCreateRoutes method
        sb.AppendLine("        private global::SimpleRouter.IRoute? GeneratedCreateRoutes(global::System.Type routeType, params object[] parameters)");
        sb.AppendLine("        {");

        for (var i = 0; i < routes.Length; i++)
        {
            var route = routes[i];
            var keyword = i == 0 ? "if" : "else if";
            sb.AppendLine($"            {keyword} (routeType == typeof({route.FullyQualifiedName}))");

            if (route.ConstructorParams.Length == 0)
            {
                sb.AppendLine($"                return new {route.FullyQualifiedName}();");
            }
            else
            {
                var args = new StringBuilder();
                for (var j = 0; j < route.ConstructorParams.Length; j++)
                {
                    if (j > 0) args.Append(", ");
                    args.Append($"({route.ConstructorParams[j].TypeName})parameters[{j}]");
                }
                sb.AppendLine($"                return new {route.FullyQualifiedName}({args});");
            }
        }

        if (routes.Length > 0)
        {
            sb.AppendLine("            else");
            sb.AppendLine("                return null;");
        }
        else
        {
            sb.AppendLine("            return null;");
        }

        sb.AppendLine("        }");
        sb.AppendLine();

        // CreateGeneratedRouteFactory helper
        sb.AppendLine("        protected global::SimpleRouter.RouteFactory CreateGeneratedRouteFactory() => GeneratedCreateRoutes;");

        sb.AppendLine("    }");

        if (host.Namespace is not null)
        {
            sb.AppendLine("}");
        }

        spc.AddSource($"{host.ClassName}.RouteFactory.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static bool ImplementsInterface(INamedTypeSymbol type, string interfaceFullName)
    {
        return type.AllInterfaces.Any(i => i.ToDisplayString() == interfaceFullName);
    }

    private readonly record struct RouteInfo(
        string FullyQualifiedName,
        string ClassName,
        bool ImplementsIRoute,
        ImmutableArray<ConstructorParam> ConstructorParams,
        string Group,
        Location Location);

    private readonly record struct ConstructorParam(string TypeName, string Name);

    private readonly record struct FactoryHostInfo(
        string ClassName,
        string? Namespace,
        bool IsPartial,
        string Group,
        Location Location);
}
