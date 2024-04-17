namespace ConstifySql;

internal sealed class GeneratorOptions
{
    public string? Namespace { get; }
    public string? ClassName { get; }
    public string? SplitByClassesFromRoot { get; }
    public string? RootNamespace { get; }

    public GeneratorOptions(string? @namespace, string? className, string? splitByClassesFromRoot, string? rootNamespace)
    {
        Namespace = @namespace;
        ClassName = className;
        SplitByClassesFromRoot = splitByClassesFromRoot;
        RootNamespace = rootNamespace;
    }
}