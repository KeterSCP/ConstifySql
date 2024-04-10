using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ConstifySql.Tests.Utils;

public class InMemoryAdditionalText : AdditionalText
{
    public override string Path { get; }
    private readonly string _text;

    public InMemoryAdditionalText(string path, string text)
    {
        Path = path;
        _text = text;
    }

    public override SourceText GetText(CancellationToken cancellationToken = default)
    {
        return SourceText.From(_text.UseLfNewLine(), Encoding.UTF8);
    }
}