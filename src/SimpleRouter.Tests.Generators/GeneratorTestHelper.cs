using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SimpleRouter.Tests.Generators;

internal static class GeneratorTestHelper
{
    public static GeneratorDriverRunResult RunGenerator<T>(string source) where T : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var generator = new T();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation, out var outputCompilation, out var diagnostics);
        return driver.GetRunResult();
    }

    public static IEnumerable<Diagnostic> GetDiagnostics<T>(string source) where T : IIncrementalGenerator, new()
    {
        var result = RunGenerator<T>(source);
        return result.Diagnostics;
    }

    public static IEnumerable<(string HintName, string Source)> GetGeneratedSources<T>(string source) where T : IIncrementalGenerator, new()
    {
        var result = RunGenerator<T>(source);
        return result.GeneratedTrees.Select(t => (Path.GetFileName(t.FilePath), t.GetText().ToString()));
    }
}
