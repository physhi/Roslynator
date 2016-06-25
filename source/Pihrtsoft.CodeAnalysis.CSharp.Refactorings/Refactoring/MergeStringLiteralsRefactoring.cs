﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Pihrtsoft.CodeAnalysis.CSharp.Refactoring
{
    internal static class MergeStringLiteralsRefactoring
    {
        public static bool CanRefactor(RefactoringContext context, BinaryExpressionSyntax binaryExpression)
        {
            return binaryExpression.IsKind(SyntaxKind.AddExpression)
                && binaryExpression.Span == context.Span
                && StringLiteralChain.IsStringLiteralChain(binaryExpression);
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            BinaryExpressionSyntax binaryExpression,
            bool multiline = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);

            var chain = new StringLiteralChain(binaryExpression);

            LiteralExpressionSyntax newNode = (multiline)
                ? chain.MergeMultiline()
                : chain.Merge();

            root = root.ReplaceNode(
                binaryExpression,
                newNode.WithAdditionalAnnotations(Formatter.Annotation));

            return document.WithSyntaxRoot(root);
        }
    }
}