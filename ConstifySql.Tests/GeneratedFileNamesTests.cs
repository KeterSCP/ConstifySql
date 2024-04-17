using System.Collections.Immutable;
using ConstifySql.Tests.Utils;
using FluentAssertions;
using Microsoft.CodeAnalysis;

namespace ConstifySql.Tests;

public class GeneratedFileNamesTests
{
    [Fact(DisplayName = "By default file name should start with project root namespace")]
    public void ByDefaultFileNameShouldStartWithProjectRootNamespace()
    {
        const string rootNamespace = "Test.Project";

        var scripts = ImmutableArray.Create<AdditionalText>
        (
            new InMemoryAdditionalText($"{rootNamespace}/ProjectDir/Scripts.Sql/Group1/Subgroup1/FirstScript.sql", "SELECT 1;"),
            new InMemoryAdditionalText($"{rootNamespace}/ProjectDir/Scripts.Sql/Group1/Subgroup1/SecondScript.sql", "SELECT 2;"),
            new InMemoryAdditionalText($"{rootNamespace}/ProjectDir/Scripts.Sql/Group1/Subgroup2/ThirdScript.sql", "SELECT 3;"),
            new InMemoryAdditionalText($"{rootNamespace}/ProjectDir/Scripts.Sql/Group2/FourthScript.sql", "SELECT 4;")
        );

        var generatedClassesPaths = TestHelper.GetAllSyntaxTrees(
                scripts, msbuildProperties: new Dictionary<string, string>
            {
                ["RootNamespace"] = rootNamespace
            })
            .Select(st => Path.GetFileNameWithoutExtension(st.FilePath))
            .ToList();

        foreach (var path in generatedClassesPaths)
        {
            path.Should().StartWith("ProjectDir");
        }
    }

    [Fact(DisplayName = "When split by classes from root is enabled, file name should start from specified root")]
    public void WhenSplitByClassesFromRootIsEnabledFileNameShouldStartFromSpecifiedRoot()
    {
        const string rootNamespace = "Test.Project";

        var scripts = ImmutableArray.Create<AdditionalText>
        (
            new InMemoryAdditionalText($"{rootNamespace}/ProjectDir/Scripts.Sql/Group1/Subgroup1/FirstScript.sql", "SELECT 1;"),
            new InMemoryAdditionalText($"{rootNamespace}/ProjectDir/Scripts.Sql/Group1/Subgroup1/SecondScript.sql", "SELECT 2;"),
            new InMemoryAdditionalText($"{rootNamespace}/ProjectDir/Scripts.Sql/Group1/Subgroup2/ThirdScript.sql", "SELECT 3;"),
            new InMemoryAdditionalText($"{rootNamespace}/ProjectDir/Scripts.Sql/Group2/FourthScript.sql", "SELECT 4;")
        );

        var generatedClassesPaths = TestHelper.GetAllSyntaxTrees(
                scripts, msbuildProperties: new Dictionary<string, string>
            {
                ["RootNamespace"] = rootNamespace,
                ["ConstifySql_SplitByClassesFromRoot"] = $"{rootNamespace}/ProjectDir/Scripts.Sql"
            })
            .Select(st => Path.GetFileNameWithoutExtension(st.FilePath))
            .ToList();

        generatedClassesPaths[0].Should().StartWith("Group1");
        generatedClassesPaths[1].Should().StartWith("Group1");
        generatedClassesPaths[2].Should().StartWith("Group1");
        generatedClassesPaths[3].Should().StartWith("Group2");
    }
}