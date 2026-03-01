using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SimpleRouter.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public sealed class ViewLocatorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Emit attribute sources
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("RouteViewAttribute.g.cs", SourceText.From(AttributeSource.RouteViewAttribute, Encoding.UTF8));
            ctx.AddSource("GeneratedViewLocatorAttribute.g.cs", SourceText.From(AttributeSource.GeneratedViewLocatorAttribute, Encoding.UTF8));
        });

        // Collect [RouteView(typeof(T))]-marked classes
        var viewMappings = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeSource.RouteViewAttributeName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => GetViewMappingInfo(ctx))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!.Value);

        // Collect [GeneratedViewLocator]-marked classes
        var locatorHosts = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeSource.GeneratedViewLocatorAttributeName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => GetLocatorHostInfo(ctx))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!.Value);

        // Combine
        var combined = locatorHosts.Combine(viewMappings.Collect());

        context.RegisterSourceOutput(combined, static (spc, pair) =>
        {
            var (host, mappings) = pair;

            if (!host.IsPartial)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.ViewLocatorClassMustBePartial,
                    host.Location,
                    host.ClassName));
                return;
            }

            if (!host.ExtendsViewLocatorBase)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.ViewLocatorClassMustExtendViewLocatorBase,
                    host.Location,
                    host.ClassName));
                return;
            }

            // Report warnings for views that don't extend Control
            foreach (var mapping in mappings)
            {
                if (!mapping.ViewExtendsControl)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.RouteViewClassShouldExtendControl,
                        mapping.Location,
                        mapping.ViewClassName));
                }
            }

            var hostGroup = host.Group;
            var filteredMappings = mappings.Where(m => m.Group == hostGroup).ToImmutableArray();
            GenerateViewLocator(spc, host, filteredMappings);
        });
    }

    private static ViewMappingInfo? GetViewMappingInfo(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol viewType)
            return null;

        // Get the route type from the attribute argument
        var attr = ctx.Attributes.FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString() == "SimpleRouter.RouteViewAttribute");

        if (attr is null || attr.ConstructorArguments.Length == 0)
            return null;

        if (attr.ConstructorArguments[0].Value is not INamedTypeSymbol routeType)
            return null;

        // Extract Group from the [RouteView] attribute's named arguments
        var group = attr.NamedArguments.FirstOrDefault(kvp => kvp.Key == "Group").Value.Value as string ?? "";

        var viewExtendsControl = ExtendsType(viewType, "Avalonia.Controls.Control");

        return new ViewMappingInfo(
            viewType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            viewType.Name,
            routeType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            routeType.Name,
            viewExtendsControl,
            group,
            ctx.TargetNode.GetLocation());
    }

    private static LocatorHostInfo? GetLocatorHostInfo(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol)
            return null;

        var classDecl = (ClassDeclarationSyntax)ctx.TargetNode;
        var isPartial = classDecl.Modifiers.Any(SyntaxKind.PartialKeyword);

        var extendsViewLocatorBase = ExtendsType(typeSymbol, "SimpleRouter.Avalonia.ViewLocatorBase");

        // Extract Group from the [GeneratedViewLocator] attribute's named arguments
        var attr = ctx.Attributes.FirstOrDefault(a =>
            a.AttributeClass?.ToDisplayString() == "SimpleRouter.GeneratedViewLocatorAttribute");
        var group = attr?.NamedArguments.FirstOrDefault(kvp => kvp.Key == "Group").Value.Value as string ?? "";

        var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : typeSymbol.ContainingNamespace.ToDisplayString();

        return new LocatorHostInfo(
            typeSymbol.Name,
            ns,
            isPartial,
            extendsViewLocatorBase,
            group,
            ctx.TargetNode.GetLocation());
    }

    private static void GenerateViewLocator(SourceProductionContext spc, LocatorHostInfo host, ImmutableArray<ViewMappingInfo> mappings)
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
        sb.AppendLine("        public override global::Avalonia.Controls.Control? ResolveControl(global::SimpleRouter.IRoute? route)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (route is null) return DefaultContent;");
        sb.AppendLine("            return route switch");
        sb.AppendLine("            {");

        foreach (var mapping in mappings)
        {
            sb.AppendLine($"                {mapping.RouteFullyQualifiedName} r => new {mapping.ViewFullyQualifiedName} {{ DataContext = r }},");
        }

        sb.AppendLine("                _ => DefaultContent,");
        sb.AppendLine("            };");
        sb.AppendLine("        }");
        sb.AppendLine("    }");

        if (host.Namespace is not null)
        {
            sb.AppendLine("}");
        }

        spc.AddSource($"{host.ClassName}.ViewLocator.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static bool ExtendsType(INamedTypeSymbol type, string baseTypeFullName)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            if (current.ToDisplayString() == baseTypeFullName)
                return true;
            current = current.BaseType;
        }
        return false;
    }

    private readonly record struct ViewMappingInfo(
        string ViewFullyQualifiedName,
        string ViewClassName,
        string RouteFullyQualifiedName,
        string RouteClassName,
        bool ViewExtendsControl,
        string Group,
        Location Location);

    private readonly record struct LocatorHostInfo(
        string ClassName,
        string? Namespace,
        bool IsPartial,
        bool ExtendsViewLocatorBase,
        string Group,
        Location Location);
}
