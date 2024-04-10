using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConstifySql.Tests.Utils;

public static class TestHelper
{
    public static IEnumerable<SyntaxTree> GetAllSyntaxTrees(ImmutableArray<AdditionalText> additionalTexts)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "ConstifySql.Tests",
            syntaxTrees: []);

        var generator = new SqlGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.AddAdditionalTexts(additionalTexts);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        return outputCompilation.SyntaxTrees;
    }

    public static IEnumerable<SyntaxTree> GetAllSyntaxTrees(ImmutableArray<AdditionalText> additionalTexts, Dictionary<string, string> msbuildProperties)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "ConstifySql.Tests",
            syntaxTrees: []);

        var properties = msbuildProperties.ToImmutableDictionary(x => $"build_property.{x.Key}", x => x.Value);

        var generator = new SqlGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver
            .Create(generator)
            .WithUpdatedAnalyzerConfigOptions(new MyAnalyzerConfigOptionsProvider(ImmutableDictionary<object, AnalyzerConfigOptions>.Empty, new MyAnalyzerConfigOptions(properties)))
            .AddAdditionalTexts(additionalTexts);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        return outputCompilation.SyntaxTrees;
    }
}