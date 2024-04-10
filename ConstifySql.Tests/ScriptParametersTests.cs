using System.Collections.Immutable;
using ConstifySql.Tests.Utils;
using Microsoft.CodeAnalysis;

namespace ConstifySql.Tests;

public class ScriptParametersTests : VerifyTestBase
{
    [Fact(DisplayName = "Should correctly extract parameters starting with @")]
    public async Task ShouldExtractParametersStartingWithAt()
    {
        var scripts = ImmutableArray.Create<AdditionalText>
        (
            new InMemoryAdditionalText
            (
                "FirstScript.sql",
                """
                SELECT * FROM users u
                WHERE id = @id AND name = @name
                ORDER by @SomeColumn;
                """
            )
        );

        var generatedClassesTexts = TestHelper.GetAllSyntaxTrees(scripts)
            .Select(st => st.GetText().ToString())
            .Single();

        await Verify(generatedClassesTexts, GetSettings());
    }

    [Fact(DisplayName = "Should correctly extract parameters starting with :")]
    public async Task ShouldExtractParametersStartingWithColon()
    {
        var scripts = ImmutableArray.Create<AdditionalText>
        (
            new InMemoryAdditionalText
            (
                "SecondScript.sql",
                """
                SELECT * FROM users u
                WHERE id = :id AND name = :name
                ORDER by :SomeColumn;
                """
            )
        );

        var generatedClassesTexts = TestHelper.GetAllSyntaxTrees(scripts)
            .Select(st => st.GetText().ToString())
            .Single();

        await Verify(generatedClassesTexts, GetSettings());
    }

    [Fact(DisplayName = "Should correctly extract parameters starting with both @ and :")]
    public async Task ShouldExtractParametersStartingWithBothAtAndColon()
    {
        var scripts = ImmutableArray.Create<AdditionalText>
        (
            new InMemoryAdditionalText
            (
                "ThirdScript.sql",
                """
                SELECT * FROM users u
                WHERE id = :id AND name = @name
                ORDER by :SomeColumn;
                """
            )
        );

        var generatedClassesTexts = TestHelper.GetAllSyntaxTrees(scripts)
            .Select(st => st.GetText().ToString())
            .Single();

        await Verify(generatedClassesTexts, GetSettings());
    }
}