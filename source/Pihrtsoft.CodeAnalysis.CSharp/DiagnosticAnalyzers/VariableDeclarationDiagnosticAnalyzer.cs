﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Pihrtsoft.CodeAnalysis.CSharp.Analysis;

namespace Pihrtsoft.CodeAnalysis.CSharp.DiagnosticAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class VariableDeclarationDiagnosticAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.DeclareExplicitType,
                    DiagnosticDescriptors.DeclareExplicitTypeEvenIfObvious,
                    DiagnosticDescriptors.DeclareImplicitType);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.RegisterSyntaxNodeAction(f => AnalyzeVariableDeclaration(f), SyntaxKind.VariableDeclaration);
        }

        private void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (GeneratedCodeAnalyzer?.IsGeneratedCode(context) == true)
                return;

            var variableDeclaration = (VariableDeclarationSyntax)context.Node;

            TypeAnalysisResult result = VariableDeclarationAnalysis.AnalyzeType(
                variableDeclaration,
                context.SemanticModel,
                context.CancellationToken);

            switch (result)
            {
                case TypeAnalysisResult.Explicit:
                    {
                        break;
                    }
                case TypeAnalysisResult.ExplicitButShouldBeImplicit:
                    {
                        context.ReportDiagnostic(
                            DiagnosticDescriptors.DeclareImplicitType,
                            variableDeclaration.Type.GetLocation());

                        break;
                    }
                case TypeAnalysisResult.Implicit:
                    {
                        context.ReportDiagnostic(
                            DiagnosticDescriptors.DeclareExplicitTypeEvenIfObvious,
                            variableDeclaration.Type.GetLocation());

                        break;
                    }
                case TypeAnalysisResult.ImplicitButShouldBeExplicit:
                    {
                        context.ReportDiagnostic(
                            DiagnosticDescriptors.DeclareExplicitType,
                            variableDeclaration.Type.GetLocation());

                        break;
                    }
            }
        }
    }
}