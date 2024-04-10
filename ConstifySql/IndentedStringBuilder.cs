using System.Text;

namespace ConstifySql;

// Copy of https://github.com/dotnet/efcore/blob/4ecf26f7be6fb92eeee54a9191438f8fe7596a9f/src/EFCore/Infrastructure/IndentedStringBuilder.cs

internal class IndentedStringBuilder
{
    private const byte IndentSize = 4;
    private int _indent;
    private bool _indentPending = true;

    private readonly StringBuilder _stringBuilder = new();

    public IndentedStringBuilder AppendLine(string value)
    {
        if (value.Length != 0)
        {
            DoIndent();
        }

        _stringBuilder.AppendLine(value);

        _indentPending = true;

        return this;
    }

    public IndentedStringBuilder IncrementIndent()
    {
        _indent++;

        return this;
    }

    public IndentedStringBuilder DecrementIndent()
    {
        if (_indent > 0)
        {
            _indent--;
        }

        return this;
    }

    public override string ToString() => _stringBuilder.ToString();

    private void DoIndent()
    {
        if (_indentPending && _indent > 0)
        {
            _stringBuilder.Append(' ', _indent * IndentSize);
        }

        _indentPending = false;
    }
}