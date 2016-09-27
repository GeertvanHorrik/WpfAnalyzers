﻿namespace WpfAnalyzers.DependencyProperties
{
    using System;
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WA1205MustRegisterForContainingType : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WA1204";
        private const string Title = "DependencyProperty must be registered for containing type.";
        private const string MessageFormat = "Property '{0}' must be registered for {1}";
        private const string Description = Title;
        private const string HelpLink = "http://stackoverflow.com/";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
                                                                      DiagnosticId,
                                                                      Title,
                                                                      MessageFormat,
                                                                      AnalyzerCategory.DependencyProperties,
                                                                      DiagnosticSeverity.Error,
                                                                      AnalyzerConstants.EnabledByDefault,
                                                                      Description,
                                                                      HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.FieldDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = context.Node as FieldDeclarationSyntax;
            throw new NotImplementedException("message");
            
            //if (declaration == null || declaration.IsMissing || !declaration.IsDependencyPropertyField())
            //{
            //    return;
            //}

            //var keyField = declaration.DependencyPropertyKey();
            //if (keyField == null)
            //{
            //    return;
            //}

            //if (keyField.SpanStart < declaration.SpanStart)
            //{
            //    return;
            //}

            //context.ReportDiagnostic(Diagnostic.Create(Descriptor, declaration.GetLocation(), declaration.Name()));
        }
    }
}