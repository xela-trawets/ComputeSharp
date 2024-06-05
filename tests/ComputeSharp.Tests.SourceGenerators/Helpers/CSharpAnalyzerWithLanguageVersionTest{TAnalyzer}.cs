extern alias Core;
#if D2D1_TESTS
extern alias D2D1;
#else
extern alias D3D12;
#endif

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis;

namespace ComputeSharp.Tests.SourceGenerators.Helpers;

/// <summary>
/// A custom <see cref="CSharpAnalyzerTest{TAnalyzer, TVerifier}"/> that uses a specific C# language version to parse code.
/// </summary>
/// <typeparam name="TAnalyzer">The type of the analyzer to test.</typeparam>
internal sealed class CSharpAnalyzerWithLanguageVersionTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, MSTestVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    /// <summary>
    /// The C# language version to use to parse code.
    /// </summary>
    private readonly LanguageVersion languageVersion;

    /// <summary>
    /// Creates a new <see cref="CSharpAnalyzerWithLanguageVersionTest{TAnalyzer}"/> instance with the specified paramaters.
    /// </summary>
    /// <param name="languageVersion">The C# language version to use to parse code.</param>
    private CSharpAnalyzerWithLanguageVersionTest(LanguageVersion languageVersion)
    {
        this.languageVersion = languageVersion;
    }

    /// <inheritdoc/>
    protected override ParseOptions CreateParseOptions()
    {
        return new CSharpParseOptions(this.languageVersion, DocumentationMode.Diagnose);
    }

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.VerifyAnalyzerAsync"/>
    /// <param name="source">The source code to analyze.</param>
    /// <param name="languageVersion">The language version to use to run the test.</param>
    public static Task VerifyAnalyzerAsync(string source, LanguageVersion languageVersion = LanguageVersion.CSharp12)
    {
        CSharpAnalyzerWithLanguageVersionTest<TAnalyzer> test = new(languageVersion) { TestCode = source };

        test.TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(Core::ComputeSharp.Hlsl).Assembly.Location));
#if D2D1_TESTS
        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(D2D1::ComputeSharp.D2D1.ID2D1PixelShader).Assembly.Location));
#else
        test.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(D3D12::ComputeSharp.IComputeShader).Assembly.Location));
#endif

        return test.RunAsync(CancellationToken.None);
    }
}