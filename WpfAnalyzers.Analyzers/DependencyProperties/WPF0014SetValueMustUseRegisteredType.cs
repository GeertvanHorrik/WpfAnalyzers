﻿namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0014SetValueMustUseRegisteredType : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0014";

        private const string Title = "SetValue must use registered type.";

        private const string MessageFormat = "{0} must use registered type {1}";

        private const string Description = "Use a type that matches registered type when setting the value of a DependencyProperty";

        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

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
            context.RegisterSyntaxNodeAction(HandleInvocation, SyntaxKind.InvocationExpression);
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null || context.SemanticModel == null)
            {
                return;
            }

            ArgumentSyntax property;
            ArgumentSyntax value;
            IFieldSymbol setField;
            if (DependencyObject.TryGetSetValueArguments(invocation, context.SemanticModel, context.CancellationToken, out property, out setField, out value) ||
                DependencyObject.TryGetSetCurrentValueArguments(invocation, context.SemanticModel, context.CancellationToken, out property, out setField, out value))
            {
                if (value.Expression.IsSameType(KnownSymbol.Object, context))
                {
                    return;
                }

                ITypeSymbol registeredType;
                if (DependencyProperty.TryGetRegisteredType(setField, context.SemanticModel, context.CancellationToken, out registeredType))
                {
                    if (registeredType.Is(KnownSymbol.Freezable) &&
                        value.Expression.IsSameType(KnownSymbol.Freezable, context))
                    {
                        return;
                    }

                    if (!registeredType.IsRepresentationPreservingConversion(value.Expression, context.SemanticModel, context.CancellationToken))
                    {
                        var setCall = context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken);
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, value.GetLocation(), setCall.Name, registeredType));
                    }
                }
            }
        }
    }
}