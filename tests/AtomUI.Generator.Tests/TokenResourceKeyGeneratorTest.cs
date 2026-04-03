using System.Reflection;
using AtomUI.Theme.TokenSystem;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace AtomUI.Generator.Tests;

public class TokenResourceKeyGeneratorTest
{
    [Fact]
    public async Task TestGenerateResourceKeyAsync()
    {
        var inputCode             = GeneratorFileTestDataLoader.LoadInput("TokenResourceKeyGeneratorTest.input.cs");
        var tokenResourceConstSource = GeneratorFileTestDataLoader.LoadExpected("TokenResourceKeyGeneratorTest.TokenResourceConst.g.cs");
        var controlTokenTypePoolSource = GeneratorFileTestDataLoader.LoadExpected("TokenResourceKeyGeneratorTest.ControlTokenTypePool.g.cs");
        
        TokenResourceKeyGenerator generator = new TokenResourceKeyGenerator();
        GeneratorDriver           driver    = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(CreateCompilation(inputCode), out var outputCompilation, out var diagnostics, TestContext.Current.CancellationToken);
        GeneratorDriverRunResult runResult = driver.GetRunResult();
        Console.WriteLine(runResult.Diagnostics);
        Assert.Single(runResult.Results);
        var generatedSources = runResult.Results[0].GeneratedSources;
        Assert.Equal(2, generatedSources.Length);
        var tokenResourceConstGenerated = generatedSources[0];
        Assert.Equal(tokenResourceConstSource, tokenResourceConstGenerated.SourceText.ToString());
        var controlTokenTypePoolGenerated = generatedSources[1];
        Assert.Equal(controlTokenTypePoolSource, controlTokenTypePoolGenerated.SourceText.ToString());
    }

    private static Compilation CreateCompilation(string source)
    {
        return CSharpCompilation.Create("compilation",
            [CSharpSyntaxTree.ParseText(source)],
            [MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(AbstractDesignToken).Assembly.Location)
            ],
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }
}