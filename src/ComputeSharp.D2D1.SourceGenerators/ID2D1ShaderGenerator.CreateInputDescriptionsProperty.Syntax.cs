using System;
using ComputeSharp.D2D1.__Internals;
using ComputeSharp.D2D1.SourceGenerators.Models;
using ComputeSharp.SourceGeneration.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

#pragma warning disable CS0618

namespace ComputeSharp.D2D1.SourceGenerators;

/// <inheritdoc/>
partial class ID2D1ShaderGenerator
{
    /// <inheritoc/>
    private static partial class InputDescriptions
    {
        /// <summary>
        /// Creates a <see cref="PropertyDeclarationSyntax"/> instance for the <c>InputDescriptions</c> property.
        /// </summary>
        /// <param name="inputDescriptions">The input descriptions info gathered for the current shader.</param>
        /// <param name="additionalDataMembers">Any additional <see cref="MemberDeclarationSyntax"/> instances needed by the generated code, if needed.</param>
        /// <returns>The resulting <see cref="PropertyDeclarationSyntax"/> instance for the <c>InputDescriptions</c> property.</returns>
        public static PropertyDeclarationSyntax GetSyntax(EquatableArray<InputDescription> inputDescriptions, out MemberDeclarationSyntax[] additionalDataMembers)
        {
            ExpressionSyntax memoryExpression;

            // If there are no input descriptions, just return a default expression.
            // Otherwise, declare the shared array and return it from the property.
            if (inputDescriptions.Length == 0)
            {
                memoryExpression = LiteralExpression(SyntaxKind.DefaultLiteralExpression, Token(SyntaxKind.DefaultKeyword));
                additionalDataMembers = Array.Empty<MemberDeclarationSyntax>();
            }
            else
            {
                memoryExpression = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("Data"),
                    IdentifierName(nameof(InputDescriptions)));

                additionalDataMembers = new[] { GetArrayDeclaration(inputDescriptions) };
            }

            // This code produces a method declaration as follows:
            //
            // readonly global::System.ReadOnlyMemory<global::ComputeSharp.D2D1.Interop.D2D1InputDescription> global::ComputeSharp.D2D1.__Internals.ID2D1Shader.InputDescriptions => <EXPRESSION>;
            return
                PropertyDeclaration(
                    GenericName(Identifier("global::System.ReadOnlyMemory"))
                    .AddTypeArgumentListArguments(IdentifierName("global::ComputeSharp.D2D1.Interop.D2D1InputDescription")),
                    Identifier(nameof(InputDescriptions)))
                .WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(IdentifierName($"global::ComputeSharp.D2D1.__Internals.{nameof(ID2D1Shader)}")))
                .AddModifiers(Token(SyntaxKind.ReadOnlyKeyword))
                .WithExpressionBody(ArrowExpressionClause(memoryExpression))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        /// <summary>
        /// Gets the array declaration for the given input descriptions.
        /// </summary>
        /// <param name="inputDescriptions">The input descriptions info gathered for the current shader.</param>
        /// <returns>The array declaration for the given input descriptions.</returns>
        private static MemberDeclarationSyntax GetArrayDeclaration(EquatableArray<InputDescription> inputDescriptions)
        {
            using ImmutableArrayBuilder<ExpressionSyntax> inputDescriptionExpressions = ImmutableArrayBuilder<ExpressionSyntax>.Rent();

            foreach (InputDescription inputDescription in inputDescriptions)
            {
                // Create the description expression (excluding level of detail):
                //
                // new(<INDEX>, <FILTER>)
                ImplicitObjectCreationExpressionSyntax inputDescriptionExpression =
                    ImplicitObjectCreationExpression()
                    .AddArgumentListArguments(
                        Argument(LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            Literal(inputDescription.Index))),
                        Argument(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("global::ComputeSharp.D2D1.D2D1Filter"),
                                IdentifierName(inputDescription.Filter.ToString()))));

                // Add the level of detail, if needed:
                //
                // { LevelOfDetailCount = <LEVEL_OF_DETAIL_COUNT> }
                if (inputDescription.LevelOfDetailCount != 0)
                {
                    inputDescriptionExpression =
                        inputDescriptionExpression
                        .WithInitializer(
                            InitializerExpression(SyntaxKind.ObjectInitializerExpression)
                            .AddExpressions(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    IdentifierName("LevelOfDetailCount"),
                                    LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        Literal(inputDescription.LevelOfDetailCount)))));
                }

                inputDescriptionExpressions.Add(inputDescriptionExpression);
            }

            // Declare the singleton property to get the memory instance:
            //
            // /// <summary>The singleton <see cref="global::ComputeSharp.D2D1.Interop.D2D1InputDescription"/> array instance.</summary>
            // public static readonly global::ComputeSharp.D2D1.Interop.D2D1InputDescription[] InputDescriptions = { <INPUT_DESCRIPTIONS> };
            return
                FieldDeclaration(
                    VariableDeclaration(
                        ArrayType(IdentifierName("global::ComputeSharp.D2D1.Interop.D2D1InputDescription"))
                        .AddRankSpecifiers(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression()))))
                    .AddVariables(
                        VariableDeclarator(Identifier(nameof(InputDescriptions)))
                        .WithInitializer(EqualsValueClause(
                            InitializerExpression(SyntaxKind.ArrayInitializerExpression)
                            .AddExpressions(inputDescriptionExpressions.ToArray())))))
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.StaticKeyword),
                    Token(SyntaxKind.ReadOnlyKeyword))
                .WithLeadingTrivia(Comment("""/// <summary>The singleton <see cref="global::ComputeSharp.D2D1.Interop.D2D1InputDescription"/> array instance.</summary>"""));
        }

        /// <summary>
        /// Gets any type declarations for additional members.
        /// </summary>
        /// <param name="memberDeclarations">The additional members that are needed.</param>
        /// <returns>Any type declarations for additional members.</returns>
        public static TypeDeclarationSyntax[] GetDataTypeDeclarations(MemberDeclarationSyntax[] memberDeclarations)
        {
            if (memberDeclarations.Length == 0)
            {
                return Array.Empty<TypeDeclarationSyntax>();
            }

            // Create the container type declaration:
            //
            // /// <summary>
            // /// A container type for additional data needed by the shader.
            // /// </summary>
            // [global::System.CodeDom.Compiler.GeneratedCode("...", "...")]
            // [global::System.Diagnostics.DebuggerNonUserCode]
            // [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
            // file static class Data
            // {
            //     <FIELD_DECLARATION>
            // }
            TypeDeclarationSyntax dataTypeDeclaration =
                ClassDeclaration("Data")
                .AddModifiers(Token(SyntaxKind.FileKeyword), Token(SyntaxKind.StaticKeyword))
                .AddAttributeLists(
                    AttributeList(SingletonSeparatedList(
                        Attribute(IdentifierName("global::System.CodeDom.Compiler.GeneratedCode")).AddArgumentListArguments(
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ID2D1ShaderGenerator).FullName))),
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(typeof(ID2D1ShaderGenerator).Assembly.GetName().Version.ToString())))))),
                    AttributeList(SingletonSeparatedList(Attribute(IdentifierName("global::System.Diagnostics.DebuggerNonUserCode")))),
                    AttributeList(SingletonSeparatedList(Attribute(IdentifierName("global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage")))))
                .AddMembers(memberDeclarations)
                .WithLeadingTrivia(
                    Comment("/// <summary>"),
                    Comment("/// A container type for input descriptions."),
                    Comment("/// </summary>"));

            return new[] { dataTypeDeclaration };
        }
    }
}