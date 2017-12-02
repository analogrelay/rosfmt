using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Xunit;

namespace Rosfmt.Engine.Tests
{
    public static class RuleTest
    {
        public static async Task<IList<Diagnostic>> EvaluateRuleAsync(IFormattingRule rule, string code)
        {
            // Create an Ad-Hoc Workspace
            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
            var doc = project.AddDocument("TestDocument", SourceText.From(code));

            // Run the rule results
            return await rule.EvaluateAsync(doc);
        }

        public static async Task SingleErrorTestAsync(IFormattingRule rule, string code, DiagnosticDescriptor expectedDescriptor, string expectedMessage = null)
        {
            MarkupTestFile.GetSpan(code, out code, out var expectedLocation);

            var actualDiagnostics = await EvaluateRuleAsync(rule, code);

            Assert.Collection(actualDiagnostics,
                diagnostic =>
                {
                    // Check ID specially so that we get a friendly error if the descriptor doesn't match
                    Assert.Equal(expectedDescriptor.Id, diagnostic.Id);
                    Assert.Same(expectedDescriptor, diagnostic.Descriptor);

                    if (expectedMessage != null)
                    {
                        Assert.Equal(expectedMessage, diagnostic.GetMessage());
                    }

                    Assert.Equal(expectedLocation, diagnostic.Location.SourceSpan);
                });

        }

        public static Location CreateLocation(int start, int length) => Location.Create("Bogus", new TextSpan(start, length), new LinePositionSpan());
    }
}
