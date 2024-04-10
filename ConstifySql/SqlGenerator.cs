﻿using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ConstifySql;

[Generator]
public class SqlGenerator : IIncrementalGenerator
{
    private const string DefaultNamespace = "GeneratedSql";
    private const string DefaultClassName = "SqlQueries";

    private static readonly char[] DirectorySeparatorChars = ['/'];

    private const string ConstifySqlOptionsAttribute =
        """
        // This file was generated by ConstifySql source generator, any changes made to this file will be lost
        #nullable enable
        namespace ConstifySql
        {
            [System.AttributeUsage(System.AttributeTargets.Assembly)]
            public class ConstifySqlOptionsAttribute : System.Attribute
            {
                public string? Namespace { get; set; }
                public string? ClassName { get; set; }
                public string? SplitByClassesFromRoot { get; set; }
            }
        }
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("ConstifySqlOptionsAttribute.g.cs", SourceText.From(ConstifySqlOptionsAttribute, Encoding.UTF8));
        });

        var optionsAttributeProperties = context.CompilationProvider.Select((comp, _) => comp.Assembly.GetAttributes()
            .Where(a => a.AttributeClass?.Name == "ConstifySqlOptionsAttribute")
            .SelectMany(a => a.NamedArguments));

        var files = context.AdditionalTextsProvider.Where(f => f.Path.EndsWith(".sql"));

        var pathAndContents = files.Select((text, cancellationToken) =>
        {
            var content = text.GetText(cancellationToken)?.ToString() ?? "";

            return (Path: text.Path, Content: content);
        });

        var filesAndParameters = pathAndContents.Combine(optionsAttributeProperties);

        context.RegisterSourceOutput(filesAndParameters, Generate);
    }

    private static void Generate(
        SourceProductionContext context,
        ((string Path, string Content) PathAndContents, IEnumerable<KeyValuePair<string, TypedConstant>> Options) generationContext)
    {
        var fileAbsolutePath = generationContext.PathAndContents.Path;
        var fileName = Path.GetFileNameWithoutExtension(generationContext.PathAndContents.Path);
        var content = generationContext.PathAndContents.Content;
        var generatedFileName = $"{fileName}.g.cs";

        var namespaceName = generationContext.Options.FirstOrDefault(o => o.Key == "Namespace").Value.Value as string ?? DefaultNamespace;
        var className = generationContext.Options.FirstOrDefault(o => o.Key == "ClassName").Value.Value as string ?? DefaultClassName;
        var splitByClassesFromRoot = generationContext.Options.FirstOrDefault(o => o.Key == "SplitByClassesFromRoot").Value.Value as string;

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

            indentedStringBuilder.AppendLine($"public const string {fileName} = @\"{content}\";");

            foreach (var _ in folders)
            {
                indentedStringBuilder.DecrementIndent();
                indentedStringBuilder.AppendLine("}");
            }
        }
        else
        {
            indentedStringBuilder.AppendLine($"public const string {fileName} = @\"{content}\";");
        }

        indentedStringBuilder.DecrementIndent();
        indentedStringBuilder.AppendLine("}");
        indentedStringBuilder.DecrementIndent();
        indentedStringBuilder.AppendLine("}");

        context.AddSource(generatedFileName, SourceText.From(indentedStringBuilder.ToString(), Encoding.UTF8));
    }
}