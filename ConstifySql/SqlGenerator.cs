﻿using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace ConstifySql;

[Generator]
public class SqlGenerator : IIncrementalGenerator
{
    private const string DefaultNamespace = "ConstifySql";
    private const string DefaultClassName = "SqlQueries";

    private static readonly char[] DirectorySeparatorChars = ['/'];
    // TODO: this could be parameterized from options
    private static readonly Regex QueryParametersRegex = new(@"[@:]\w+", RegexOptions.Compiled);
    private static readonly Dictionary<string, int> GeneratedFileNames = new();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var customParameters = context.AnalyzerConfigOptionsProvider
            .Select((configOptions, _) =>
            {
                configOptions.GlobalOptions.TryGetValue("build_property.ConstifySql_Namespace", out var namespaceConfig);
                configOptions.GlobalOptions.TryGetValue("build_property.ConstifySql_ClassName", out var classNameConfig);
                configOptions.GlobalOptions.TryGetValue("build_property.ConstifySql_SplitByClassesFromRoot", out var splitByClassesFromRootConfig);

                return (Namespace: namespaceConfig, ClassName: classNameConfig, SplitByClassesFromRoot: splitByClassesFromRootConfig);
            });

        var files = context.AdditionalTextsProvider.Where(f => f.Path.EndsWith(".sql"));

        var pathAndContents = files.Select((text, cancellationToken) =>
        {
            var content = text.GetText(cancellationToken)?.ToString() ?? "";

            return (Path: text.Path, Content: content);
        });

        var filesAndParameters = pathAndContents.Combine(customParameters);

        context.RegisterSourceOutput(filesAndParameters, Generate);
    }

    private static void Generate(
        SourceProductionContext context,
        ((string Path, string Content) PathAndContents, (string? Namespace, string? ClassName, string? SplitByClassesFromRoot) Options) generationContext)
    {
        var fileAbsolutePath = generationContext.PathAndContents.Path;
        var fileName = Path.GetFileNameWithoutExtension(generationContext.PathAndContents.Path);
        var content = generationContext.PathAndContents.Content;
        var generatedFileName = $"{GetGeneratedFileName(fileName)}.g.cs";
        var queryParameters = QueryParametersRegex.Matches(content)
            .Cast<Match>()
            .Select(m => m.Value)
            .ToArray();

        var namespaceName = string.IsNullOrWhiteSpace(generationContext.Options.Namespace) ? DefaultNamespace : generationContext.Options.Namespace;
        var className = string.IsNullOrWhiteSpace(generationContext.Options.ClassName) ? DefaultClassName : generationContext.Options.ClassName;
        var splitByClassesFromRoot = generationContext.Options.SplitByClassesFromRoot;

        var indentedStringBuilder = new IndentedStringBuilder();

        indentedStringBuilder.AppendLine("// This file was generated by ConstifySql source generator, any changes made to this file will be lost");
        indentedStringBuilder.AppendLine("// ReSharper Disable All");
        indentedStringBuilder.AppendLine($"namespace {namespaceName}");
        indentedStringBuilder.AppendLine("{");
        indentedStringBuilder.IncrementIndent();
        indentedStringBuilder.AppendLine($"public static partial class {className}");
        indentedStringBuilder.AppendLine("{");
        indentedStringBuilder.IncrementIndent();

        if (!string.IsNullOrEmpty(splitByClassesFromRoot))
        {
            var normalizedRoot = splitByClassesFromRoot!.Replace('\\', '/');
            var normalizedFilePath = fileAbsolutePath.Replace('\\', '/');

            var folders = normalizedFilePath
                .Substring(normalizedFilePath.LastIndexOf(normalizedRoot, StringComparison.Ordinal) + normalizedRoot.Length)
                .Split(DirectorySeparatorChars, StringSplitOptions.RemoveEmptyEntries)
                .AsSpan();

            folders = folders.Slice(0, folders.Length - 1); // Skip last element (sql file itself)

            foreach (var folder in folders)
            {
                indentedStringBuilder.AppendLine($"public static partial class {folder}");
                indentedStringBuilder.AppendLine("{");
                indentedStringBuilder.IncrementIndent();
            }

            AppendParametersXmlDescription(indentedStringBuilder, queryParameters);
            indentedStringBuilder.AppendLine($"public const string {fileName} = {ToLiteral(content)};");

            foreach (var _ in folders)
            {
                indentedStringBuilder.DecrementIndent();
                indentedStringBuilder.AppendLine("}");
            }
        }
        else
        {
            AppendParametersXmlDescription(indentedStringBuilder, queryParameters);
            indentedStringBuilder.AppendLine($"public const string {fileName} = {ToLiteral(content)};");
        }

        indentedStringBuilder.DecrementIndent();
        indentedStringBuilder.AppendLine("}");
        indentedStringBuilder.DecrementIndent();
        indentedStringBuilder.AppendLine("}");

        context.AddSource(generatedFileName, SourceText.From(indentedStringBuilder.ToString(), Encoding.UTF8));
    }

    private static string ToLiteral(string input)
    {
        return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(input)).ToFullString();
    }

    private static void AppendParametersXmlDescription(IndentedStringBuilder sb, string[] queryParameters)
    {
        if (queryParameters.Length == 0)
        {
            return;
        }

        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Query parameters:");
        sb.AppendLine("/// <list type=\"bullet\">");

        foreach (var parameter in queryParameters)
        {
            sb.AppendLine($"/// <item><description>{parameter}</description></item>");
        }

        sb.AppendLine("/// </list>");
        sb.AppendLine("/// </summary>");
    }

    private static string GetGeneratedFileName(string fileName)
    {
        if (!GeneratedFileNames.ContainsKey(fileName))
        {
            GeneratedFileNames[fileName] = 0;
            return fileName;
        }

        GeneratedFileNames[fileName]++;
        return $"{fileName}_{GeneratedFileNames[fileName]}";
    }
}