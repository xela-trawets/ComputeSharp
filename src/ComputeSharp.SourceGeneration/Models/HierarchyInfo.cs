using System;
using ComputeSharp.SourceGeneration.Extensions;
using ComputeSharp.SourceGeneration.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.SymbolDisplayTypeQualificationStyle;

namespace ComputeSharp.SourceGeneration.Models;

/// <summary>
/// A model describing the hierarchy info for a specific type.
/// </summary>
/// <param name="FullyQualifiedMetadataName">The fully qualified metadata name for the current type.</param>
/// <param name="Namespace">Gets the namespace for the current type.</param>
/// <param name="Hierarchy">Gets the sequence of type definitions containing the current type.</param>
internal sealed partial record HierarchyInfo(string FullyQualifiedMetadataName, string Namespace, EquatableArray<TypeInfo> Hierarchy)
{
    /// <summary>
    /// Creates a new <see cref="HierarchyInfo"/> instance from a given <see cref="INamedTypeSymbol"/>.
    /// </summary>
    /// <param name="typeSymbol">The input <see cref="INamedTypeSymbol"/> instance to gather info for.</param>
    /// <returns>A <see cref="HierarchyInfo"/> instance describing <paramref name="typeSymbol"/>.</returns>
    public static HierarchyInfo From(INamedTypeSymbol typeSymbol)
    {
        using ImmutableArrayBuilder<TypeInfo> hierarchy = new();

        for (INamedTypeSymbol? parent = typeSymbol;
             parent is not null;
             parent = parent.ContainingType)
        {
            hierarchy.Add(new TypeInfo(
                parent.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                parent.TypeKind,
                parent.IsRecord));
        }

        return new(
            typeSymbol.GetFullyQualifiedMetadataName(),
            typeSymbol.ContainingNamespace.ToDisplayString(new(typeQualificationStyle: NameAndContainingTypesAndNamespaces)),
            hierarchy.ToImmutable());
    }

    /// <summary>
    /// Writes syntax for the current hierarchy into a target writer.
    /// </summary>
    /// <typeparam name="T">The type of state to pass to callbacks.</typeparam>
    /// <param name="state">The input state to pass to callbacks.</param>
    /// <param name="writer">The target <see cref="IndentedTextWriter"/> instance to write text to.</param>
    /// <param name="baseTypes">A list of base types to add to the generated type, if any.</param>
    /// <param name="memberCallbacks">The callbacks to use to write members into the declared type.</param>
    public void WriteSyntax<T>(
        T state,
        IndentedTextWriter writer,
        ReadOnlySpan<string> baseTypes,
        ReadOnlySpan<IndentedTextWriter.Callback<T>> memberCallbacks)
    {
        // Write the generated file header
        writer.WriteLine("// <auto-generated/>");
        writer.WriteLine("#pragma warning disable");
        writer.WriteLine();

        // Declare the namespace, if needed
        if (Namespace.Length > 0)
        {
            writer.WriteLine($"namespace {Namespace}");
            writer.WriteLine("{");
            writer.IncreaseIndent();
        }

        // Declare all the opening types until the inner-most one
        for (int i = Hierarchy.Length - 1; i >= 0; i--)
        {
            writer.WriteLine($$"""/// <inheritdoc cref="{{Hierarchy[i].QualifiedName}}"/>""");
            writer.Write($$"""partial {{Hierarchy[i].GetTypeKeyword()}} {{Hierarchy[i].QualifiedName}}""");

            // Add any base types, if needed
            if (i == 0 && !baseTypes.IsEmpty)
            {
                writer.Write(" : ");
                writer.WriteInitializationExpressions(baseTypes, static (item, writer) => writer.Write(item));
                writer.WriteLine();
            }
            else
            {
                writer.WriteLine();
            }

            writer.WriteLine($$"""{""");
            writer.IncreaseIndent();
        }

        // Generate all nested members
        writer.WriteLineSeparatedMembers(memberCallbacks, (callback, writer) => callback(state, writer));

        // Close all scopes and reduce the indentation
        for (int i = 0; i < Hierarchy.Length; i++)
        {
            writer.DecreaseIndent();
            writer.WriteLine("}");
        }

        // Close the namespace scope as well, if needed
        if (Namespace.Length > 0)
        {
            writer.DecreaseIndent();
            writer.WriteLine("}");
        }
    }

    /// <summary>
    /// Gets the fully qualified type name for the current instance.
    /// </summary>
    /// <returns>The fully qualified type name for the current instance.</returns>
    public string GetFullyQualifiedTypeName()
    {
        using ImmutableArrayBuilder<char> fullyQualifiedTypeName = new();

        fullyQualifiedTypeName.AddRange("global::".AsSpan());

        if (Namespace.Length > 0)
        {
            fullyQualifiedTypeName.AddRange(Namespace.AsSpan());
            fullyQualifiedTypeName.Add('.');
        }

        fullyQualifiedTypeName.AddRange(Hierarchy[^1].QualifiedName.AsSpan());

        for (int i = Hierarchy.Length - 2; i >= 0; i--)
        {
            fullyQualifiedTypeName.Add('.');
            fullyQualifiedTypeName.AddRange(Hierarchy[i].QualifiedName.AsSpan());
        }

        return fullyQualifiedTypeName.ToString();
    }

    /// <summary>
    /// Creates a <see cref="CompilationUnitSyntax"/> instance for the current hierarchy.
    /// </summary>
    /// <param name="memberDeclarations">The member declarations to add to the generated type.</param>
    /// <returns>A <see cref="CompilationUnitSyntax"/> instance for the current hierarchy.</returns>
    public CompilationUnitSyntax GetSyntax(params MemberDeclarationSyntax[] memberDeclarations)
    {
        return GetSyntax(memberDeclarations, Array.Empty<MemberDeclarationSyntax>());
    }

    /// <summary>
    /// Creates a <see cref="CompilationUnitSyntax"/> instance for the current hierarchy.
    /// </summary>
    /// <param name="memberDeclarations">The member declarations to add to the generated type.</param>
    /// <param name="additionalMemberDeclarations">Additional top-level member declarations, if any.</param>
    /// <returns>A <see cref="CompilationUnitSyntax"/> instance for the current hierarchy.</returns>
    public CompilationUnitSyntax GetSyntax(MemberDeclarationSyntax[] memberDeclarations, params MemberDeclarationSyntax[] additionalMemberDeclarations)
    {
        // Create the partial type declaration with for the current hierarchy.
        // This code produces a type declaration as follows:
        //
        // partial <TYPE_KIND> <TYPE_NAME>
        // {
        //     <MEMBER_DECLARATIONS>
        // }
        TypeDeclarationSyntax typeDeclarationSyntax =
            Hierarchy[0].GetSyntax()
            .AddModifiers(Token(SyntaxKind.PartialKeyword))
            .AddMembers(memberDeclarations);

        // Add all parent types in ascending order, if any
        foreach (TypeInfo parentType in Hierarchy.AsSpan().Slice(1))
        {
            typeDeclarationSyntax =
                parentType.GetSyntax()
                .AddModifiers(Token(SyntaxKind.PartialKeyword))
                .AddMembers(typeDeclarationSyntax);
        }

        // Prepare the leading trivia for the generated compilation unit.
        // This will produce code as follows:
        //
        // // <auto-generated/>
        // #pragma warning disable
        SyntaxTriviaList syntaxTriviaList = TriviaList(
            Comment("// <auto-generated/>"),
            Trivia(PragmaWarningDirectiveTrivia(Token(SyntaxKind.DisableKeyword), true)));

        CompilationUnitSyntax compilationUnitSyntax;

        if (Namespace is "")
        {
            // If there is no namespace, attach the pragma directly to the declared type,
            // and skip the namespace declaration. This will produce code as follows:
            //
            // <SYNTAX_TRIVIA>
            // <TYPE_HIERARCHY>
            compilationUnitSyntax =
                CompilationUnit()
                .AddMembers(typeDeclarationSyntax.WithLeadingTrivia(syntaxTriviaList));
        }
        else
        {
            // Create the compilation unit with disabled warnings, target namespace and generated type.
            // This will produce code as follows:
            //
            // <SYNTAX_TRIVIA>
            // namespace <NAMESPACE>;
            // 
            // <TYPE_HIERARCHY>
            compilationUnitSyntax =
                CompilationUnit().AddMembers(
                FileScopedNamespaceDeclaration(IdentifierName(Namespace))
                .WithLeadingTrivia(syntaxTriviaList)
                .AddMembers(typeDeclarationSyntax));
        }

        // Add any additional members, if any
        if (additionalMemberDeclarations.Length > 0)
        {
            compilationUnitSyntax = compilationUnitSyntax.AddMembers(additionalMemberDeclarations);
        }

        // Normalize and return the tree
        return compilationUnitSyntax.NormalizeWhitespace(eol: "\n");
    }
}