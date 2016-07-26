﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Pihrtsoft.CodeAnalysis.CSharp.Refactoring
{
    internal static class ExtractGenericTypeRefactoring
    {
        public static bool CanRefactor(RefactoringContext context, GenericNameSyntax genericName)
        {
            return genericName.TypeArgumentList != null
                && genericName.TypeArgumentList.Arguments.Count == 1
                && genericName.TypeArgumentList.Arguments[0].Span.Contains(context.Span);
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            GenericNameSyntax genericName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken);

            TypeSyntax typeSyntax = genericName
                .TypeArgumentList
                .Arguments[0]
                .WithTriviaFrom(genericName);

            SyntaxNode newRoot = oldRoot.ReplaceNode(genericName, typeSyntax);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}