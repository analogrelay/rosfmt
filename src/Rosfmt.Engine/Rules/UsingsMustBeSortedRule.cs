using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Utilities;
using Roslyn.Utilities;

namespace Rosfmt.Rules
{
    public class UsingsMustBeSortedRule : IFormattingRule
    {
        public static readonly DiagnosticDescriptor DiagnosticDescriptor = new DiagnosticDescriptor(
            "FMT0001",
            title: "Usings must be sorted",
            messageFormat: "Must be sorted",
            category: "Formatting",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Usings in this file must be sorted.");

        private readonly bool _sortSystemFirst;

        public UsingsMustBeSortedRule(bool sortSystemFirst)
        {
            _sortSystemFirst = sortSystemFirst;
        }

        public async Task<ImmutableArray<Diagnostic>> EvaluateAsync(Document target)
        {
            var rewriter = new CSharpRewriter(
                fixIssues: false,
                comparer: _sortSystemFirst ?
                    UsingsAndExternAliasesDirectiveComparer.SystemFirstInstance :
                    UsingsAndExternAliasesDirectiveComparer.NormalInstance);
            rewriter.Visit(await target.GetSyntaxRootAsync());
            return rewriter.Diagnostics;
        }

        private class CSharpRewriter : CSharpSyntaxRewriter
        {
            private readonly bool _fixIssues;
            private readonly IComparer<SyntaxNode> _comparer;
            private ImmutableArray<Diagnostic>.Builder _diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            public ImmutableArray<Diagnostic> Diagnostics => _diagnostics.ToImmutable();

            public CSharpRewriter(bool fixIssues, IComparer<SyntaxNode> comparer)
            {
                _fixIssues = fixIssues;
                _comparer = comparer;
            }

            public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
            {
                CheckUsings(node, node.Usings);

                return base.VisitCompilationUnit(node);
            }

            private void CheckUsings(SyntaxNode node, SyntaxList<UsingDirectiveSyntax> usings)
            {
                if (!usings.IsSorted(_comparer))
                {
                    _diagnostics.Add(Diagnostic.Create(
                        DiagnosticDescriptor,
                        Location.Create(node.SyntaxTree, usings.Span)));
                }
            }

            public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
            {
                CheckUsings(node, node.Usings);

                return base.VisitNamespaceDeclaration(node);
            }
        }
    }
}
