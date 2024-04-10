using System.Collections.Immutable;
using ConstifySql.Tests.Utils;
using Microsoft.CodeAnalysis;

namespace ConstifySql.Tests;

public class GeneratorConfigurationTests : VerifyTestBase
{
    [Fact(DisplayName = "Should correctly generate POCO with custom namespace")]
    public async Task ShouldGeneratePocoWithCustomNamespace()
    {
        var scripts = ImmutableArray.Create<AdditionalText>
        (
            new InMemoryAdditionalText("FirstScript.sql", "SELECT * FROM Table1;")
        );

        var generatedClassesTexts = TestHelper.GetAllSyntaxTrees
            (
                scripts,
                msbuildProperties: new Dictionary<string, string>
                {
                    ["ConstifySql_Namespace"] = "CustomNamespace"
                }
            )
            .Select(st => st.GetText().ToString())
            .Single();

        await Verify(generatedClassesTexts, GetSettings());
    }

    [Fact(DisplayName = "Should correctly generate POCO with custom class name")]
    public async Task ShouldGeneratePocoWithCustomClassName()
    {
        var scripts = ImmutableArray.Create<AdditionalText>
        (
            new InMemoryAdditionalText("FirstScript.sql", "SELECT * FROM Table1;")
        );

        var generatedClassesTexts = TestHelper.GetAllSyntaxTrees
            (
                scripts,
                msbuildProperties: new Dictionary<string, string>
                {
                    ["ConstifySql_ClassName"] = "CustomClassName"
                }
            )
            .Select(st => st.GetText().ToString())
            .Single();

        await Verify(generatedClassesTexts, GetSettings());
    }

    [Fact(DisplayName = "Should correctly split POCO by classes from root")]
    public async Task ShouldSplitPocoByClassesFromRoot()
    {
        var scripts = ImmutableArray.Create<AdditionalText>
        (
            new InMemoryAdditionalText("ProjectDir/Scripts/Group1/Subgroup1/FirstScript.sql", "SELECT 1;"),
            new InMemoryAdditionalText("ProjectDir/Scripts/Group1/Subgroup1/SecondScript.sql", "SELECT 2;"),
            new InMemoryAdditionalText("ProjectDir/Scripts/Group1/Subgroup2/ThirdScript.sql", "SELECT 3;"),
            new InMemoryAdditionalText("ProjectDir/Scripts/Group2/FourthScript.sql", "SELECT 4;")
        );

        var generatedClassesTexts = TestHelper.GetAllSyntaxTrees
            (
                scripts,
                msbuildProperties: new Dictionary<string, string>
                {
                    ["ConstifySql_SplitByClassesFromRoot"] = "ProjectDir/Scripts/"
                }
            )
            .Select(st => st.GetText().ToString())
            .ToList();

        await Verify(generatedClassesTexts, GetSettings());
    }
}