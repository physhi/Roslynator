﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.Extensions;

namespace Roslynator.CSharp.Refactorings.ExtractCondition
{
    internal class ExtractConditionFromIfToIfRefactoring
        : ExtractConditionFromIfRefactoring
    {
        public override string Title
        {
            get { return "Extract condition to if"; }
        }

        public async Task<Document> RefactorAsync(
            Document document,
            StatementContainer container,
            BinaryExpressionSyntax condition,
            ExpressionSyntax expression,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var ifStatement = (IfStatementSyntax)condition.Parent;

            IfStatementSyntax newIfStatement = RemoveExpressionFromCondition(ifStatement, condition, expression)
                .WithFormatterAnnotation();

            SyntaxNode newNode = AddNextIf(container, ifStatement, newIfStatement, expression);

            return await document.ReplaceNodeAsync(container.Node, newNode, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Document> RefactorAsync(
            Document document,
            StatementContainer container,
            BinaryExpressionSyntax condition,
            SelectedExpressions selectedExpressions,
            CancellationToken cancellationToken)
        {
            var ifStatement = (IfStatementSyntax)condition.Parent;

            IfStatementSyntax newIfStatement = RemoveExpressionsFromCondition(ifStatement, condition, selectedExpressions)
                .WithFormatterAnnotation();

            ExpressionSyntax expression = SyntaxFactory.ParseExpression(selectedExpressions.ExpressionsText);

            SyntaxNode newNode = AddNextIf(container, ifStatement, newIfStatement, expression);

            return await document.ReplaceNodeAsync(container.Node, newNode, cancellationToken).ConfigureAwait(false);
        }

        private static SyntaxNode AddNextIf(
            StatementContainer container,
            IfStatementSyntax ifStatement,
            IfStatementSyntax newIfStatement,
            ExpressionSyntax expression)
        {
            IfStatementSyntax nextIfStatement = ifStatement.WithCondition(expression)
                .WithFormatterAnnotation();

            SyntaxList<StatementSyntax> statements = container.Statements;

            int index = statements.IndexOf(ifStatement);

            SyntaxList<StatementSyntax> newStatements = statements
                .Replace(ifStatement, newIfStatement)
                .Insert(index + 1, nextIfStatement);

            return container.NodeWithStatements(newStatements);
        }
    }
}
