﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Extensions;
using Roslynator.Extensions;

namespace Roslynator.CSharp.Refactorings
{
    internal static class FormatDeclarationBracesRefactoring
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration)
        {
            if (!classDeclaration.Members.Any())
                Analyze(context, classDeclaration, classDeclaration.OpenBraceToken, classDeclaration.CloseBraceToken);
        }

        public static void Analyze(SyntaxNodeAnalysisContext context, StructDeclarationSyntax structDeclaration)
        {
            if (!structDeclaration.Members.Any())
                Analyze(context, structDeclaration, structDeclaration.OpenBraceToken, structDeclaration.CloseBraceToken);
        }

        public static void Analyze(SyntaxNodeAnalysisContext context, InterfaceDeclarationSyntax interfaceDeclaration)
        {
            if (!interfaceDeclaration.Members.Any())
                Analyze(context, interfaceDeclaration, interfaceDeclaration.OpenBraceToken, interfaceDeclaration.CloseBraceToken);
        }

        private static void Analyze(
            SyntaxNodeAnalysisContext context,
            MemberDeclarationSyntax declaration,
            SyntaxToken openBrace,
            SyntaxToken closeBrace)
        {
            if (!openBrace.IsMissing
                && !closeBrace.IsMissing
                && declaration.SyntaxTree.GetLineCount(TextSpan.FromBounds(openBrace.Span.End, closeBrace.SpanStart)) != 2)
            {
                TextSpan span = TextSpan.FromBounds(openBrace.Span.Start, closeBrace.Span.End);

                if (declaration
                    .DescendantTrivia(span)
                    .All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                {
                    context.ReportDiagnostic(
                        DiagnosticDescriptors.FormatDeclarationBraces,
                        Location.Create(declaration.SyntaxTree, span));
                }
            }
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            MemberDeclarationSyntax declaration,
            CancellationToken cancellationToken)
        {
            MemberDeclarationSyntax newNode = GetNewDeclaration(declaration)
                .WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(declaration, newNode, cancellationToken).ConfigureAwait(false);
        }

        private static MemberDeclarationSyntax GetNewDeclaration(MemberDeclarationSyntax declaration)
        {
            switch (declaration.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                    {
                        var classDeclaration = (ClassDeclarationSyntax)declaration;

                        return classDeclaration
                            .WithOpenBraceToken(classDeclaration.OpenBraceToken.WithoutTrailingTrivia())
                            .WithCloseBraceToken(classDeclaration.CloseBraceToken.WithLeadingTrivia(CSharpFactory.NewLineTrivia()));
                    }
                case SyntaxKind.StructDeclaration:
                    {
                        var structDeclaration = (StructDeclarationSyntax)declaration;

                        return structDeclaration
                            .WithOpenBraceToken(structDeclaration.OpenBraceToken.WithoutTrailingTrivia())
                            .WithCloseBraceToken(structDeclaration.CloseBraceToken.WithLeadingTrivia(CSharpFactory.NewLineTrivia()));
                    }
                case SyntaxKind.InterfaceDeclaration:
                    {
                        var interfaceDeclaration = (InterfaceDeclarationSyntax)declaration;

                        return interfaceDeclaration
                            .WithOpenBraceToken(interfaceDeclaration.OpenBraceToken.WithoutTrailingTrivia())
                            .WithCloseBraceToken(interfaceDeclaration.CloseBraceToken.WithLeadingTrivia(CSharpFactory.NewLineTrivia()));
                    }
            }

            return declaration;
        }
    }
}
