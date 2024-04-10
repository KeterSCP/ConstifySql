using System.Collections.Immutable;
using ConstifySql.Tests.Utils;
using Microsoft.CodeAnalysis;

namespace ConstifySql.Tests;

public class DefaultGenerationTests : VerifyTestBase
{
    [Fact(DisplayName = "Should correctly generate POCO with default structure")]
    public async Task ShouldGeneratePocoWithDefaultStructure()
    {
        var scripts = ImmutableArray.Create<AdditionalText>
        (
            new InMemoryAdditionalText("FirstScript.sql", "SELECT * FROM Table1;"),
            new InMemoryAdditionalText("SecondScript.sql", "SELECT * FROM Table2;")
        );

        var generatedClassesTexts = TestHelper.GetAllSyntaxTrees(scripts)
            .Select(st => st.GetText().ToString())
            .ToList();

        await Verify(generatedClassesTexts, GetSettings());
    }
}