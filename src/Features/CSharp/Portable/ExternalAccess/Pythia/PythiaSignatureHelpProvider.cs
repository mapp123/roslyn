﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.SignatureHelp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.SignatureHelp;
using Microsoft.CodeAnalysis.ExternalAccess.Pythia.Api;

namespace Microsoft.CodeAnalysis.ExternalAccess.Pythia
{
    /// <summary>
    /// Ensure this is ordered before the regular invocation signature help provider.
    /// We must replace the entire list of results, including both Pythia and non-Pythia recommendations.
    /// </summary>
    [ExportSignatureHelpProvider("PythiaSignatureHelpProvider", LanguageNames.CSharp), Shared]
    [ExtensionOrder(Before = "InvocationExpressionSignatureHelpProvider")]
    internal sealed class PythiaSignatureHelpProvider : InvocationExpressionSignatureHelpProviderBase
    {
        private readonly IPythiaSignatureHelpProviderImplementation _implementation;

        [ImportingConstructor]
        public PythiaSignatureHelpProvider(IPythiaSignatureHelpProviderImplementation implementation)
        {
            _implementation = implementation;
        }

        internal async override Task<(ImmutableArray<SignatureHelpItem> items, int? selectedItemIndex)> GetMethodGroupItemsAndSelectionAsync(
            ImmutableArray<IMethodSymbol> accessibleMethods,
            Document document,
            InvocationExpressionSyntax invocationExpression,
            SemanticModel semanticModel,
            SymbolInfo currentSymbol,
            CancellationToken cancellationToken)
        {
            var (items, selectedItemIndex) = await _implementation.GetMethodGroupItemsAndSelectionAsync(accessibleMethods, document, invocationExpression, semanticModel, currentSymbol, cancellationToken).ConfigureAwait(false);
            return (items.SelectAsArray(item => item.UnderlyingObject), selectedItemIndex);
        }
    }
}
