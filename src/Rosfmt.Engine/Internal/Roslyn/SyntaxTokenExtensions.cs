// This file has had some methods removed because I didn't want to pull in their dependencies and didn't need them :)

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.LanguageServices;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Extensions
{
    internal static class SyntaxTokenExtensions
    {
        public static bool IsKindOrHasMatchingText(this SyntaxToken token, SyntaxKind kind)
        {
            return token.Kind() == kind || token.HasMatchingText(kind);
        }

        public static bool HasMatchingText(this SyntaxToken token, SyntaxKind kind)
        {
            return token.ToString() == SyntaxFacts.GetText(kind);
        }

        public static bool IsKind(this SyntaxToken token, SyntaxKind kind1, SyntaxKind kind2)
        {
            return token.Kind() == kind1
                || token.Kind() == kind2;
        }

        public static bool IsKind(this SyntaxToken token, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3)
        {
            return token.Kind() == kind1
                || token.Kind() == kind2
                || token.Kind() == kind3;
        }

        public static bool IsKind(this SyntaxToken token, params SyntaxKind[] kinds)
        {
            return kinds.Contains(token.Kind());
        }

        public static bool IsLiteral(this SyntaxToken token)
        {
            switch (token.Kind())
            {
                case SyntaxKind.CharacterLiteralToken:
                case SyntaxKind.FalseKeyword:
                case SyntaxKind.NumericLiteralToken:
                case SyntaxKind.StringLiteralToken:
                case SyntaxKind.TrueKeyword:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IntersectsWith(this SyntaxToken token, int position)
        {
            return token.Span.IntersectsWith(position);
        }

        /// <summary>
        /// Determines whether the given SyntaxToken is the first token on a line in the specified SourceText.
        /// </summary>
        public static bool IsFirstTokenOnLine(this SyntaxToken token, SourceText text)
        {
            var previousToken = token.GetPreviousToken(includeSkipped: true, includeDirectives: true, includeDocumentationComments: true);
            if (previousToken.Kind() == SyntaxKind.None)
            {
                return true;
            }

            var tokenLine = text.Lines.IndexOf(token.SpanStart);
            var previousTokenLine = text.Lines.IndexOf(previousToken.SpanStart);
            return tokenLine > previousTokenLine;
        }

        public static bool SpansPreprocessorDirective(this IEnumerable<SyntaxToken> tokens)
        {
            // we want to check all leading trivia of all tokens (except the 
            // first one), and all trailing trivia of all tokens (except the
            // last one).

            var first = true;
            var previousToken = default(SyntaxToken);

            foreach (var token in tokens)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    // check the leading trivia of this token, and the trailing trivia
                    // of the previous token.
                    if (SpansPreprocessorDirective(token.LeadingTrivia) ||
                        SpansPreprocessorDirective(previousToken.TrailingTrivia))
                    {
                        return true;
                    }
                }

                previousToken = token;
            }

            return false;
        }

        private static bool SpansPreprocessorDirective(SyntaxTriviaList list)
        {
            return list.Any(t => t.GetStructure() is DirectiveTriviaSyntax);
        }

        public static bool IsRegularStringLiteral(this SyntaxToken token)
        {
            return token.Kind() == SyntaxKind.StringLiteralToken && !token.IsVerbatimStringLiteral();
        }

        public static bool IsValidAttributeTarget(this SyntaxToken token)
        {
            switch (token.Kind())
            {
                case SyntaxKind.AssemblyKeyword:
                case SyntaxKind.ModuleKeyword:
                case SyntaxKind.FieldKeyword:
                case SyntaxKind.EventKeyword:
                case SyntaxKind.MethodKeyword:
                case SyntaxKind.ParamKeyword:
                case SyntaxKind.PropertyKeyword:
                case SyntaxKind.ReturnKeyword:
                case SyntaxKind.TypeKeyword:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsOpenBraceOrCommaOfObjectInitializer(this SyntaxToken token)
        {
            return (token.IsKind(SyntaxKind.OpenBraceToken) || token.IsKind(SyntaxKind.CommaToken)) &&
                token.Parent.IsKind(SyntaxKind.ObjectInitializerExpression);
        }

        public static bool IsOpenBraceOfAccessorList(this SyntaxToken token)
        {
            return token.IsKind(SyntaxKind.OpenBraceToken) && token.Parent.IsKind(SyntaxKind.AccessorList);
        }

        /// <summary>
        /// Returns true if this token is something that looks like a C# keyword. This includes 
        /// actual keywords, contextual keywords, and even 'var' and 'dynamic'
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool CouldBeKeyword(this SyntaxToken token)
        {
            if (token.IsKeyword())
            {
                return true;
            }

            if (token.Kind() == SyntaxKind.IdentifierToken)
            {
                var simpleNameText = token.ValueText;
                return simpleNameText == "var" ||
                       simpleNameText == "dynamic" ||
                       SyntaxFacts.GetContextualKeywordKind(simpleNameText) != SyntaxKind.None;
            }

            return false;
        }
    }
}
