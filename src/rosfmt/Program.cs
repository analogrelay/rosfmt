using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Options;

namespace roslint
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            if (args.Length < 1)
            {
                return Fail("Usage: roslint <csproj-or-sln>");
            }

            var target = Path.GetFullPath(args[0]);
            var ext = Path.GetExtension(target);

            if (!File.Exists(target))
            {
                return Fail($"Target project does not exist: {target}");
            }

            // Create a workspace
            var workspace = MSBuildWorkspace.Create();

            if (string.Equals(".sln", ext))
            {
                await workspace.OpenSolutionAsync(target);
            }
            else
            {
                await workspace.OpenProjectAsync(target);
            }

            var failed = false;
            foreach (var diagnostic in workspace.Diagnostics)
            {
                failed |= diagnostic.Kind == WorkspaceDiagnosticKind.Failure;
                Console.WriteLine(diagnostic);
            }

            if (failed)
            {
                return 1;
            }

            // Load options
            var optionSet = LoadOptions(workspace.Options);

            // Format the documents
            var solution = workspace.CurrentSolution;
            var documentIds = workspace.CurrentSolution.Projects.SelectMany(p => p.DocumentIds).ToList();
            foreach (var documentId in documentIds)
            {
                // Get changes for this document
                var document = solution.GetDocument(documentId);

                var changes = Formatter.GetFormattedTextChanges(
                    await document.GetSyntaxRootAsync(),
                    workspace,
                    optionSet);

                if (changes.Count > 0)
                {
                    Console.WriteLine($"Formatting {document.FilePath} ...");
                    var text = await document.GetTextAsync();
                    var newText = text.WithChanges(changes);
                    solution = solution.WithDocumentText(document.Id, newText);
                }
            }

            if (!workspace.TryApplyChanges(solution))
            {
                return Fail("Failed to apply changes to solution");
            }

            return 0;
        }

        private static OptionSet LoadOptions(OptionSet baseOptions)
        {
            return baseOptions
                .WithChangedOption(CSharpFormattingOptions.SpaceAfterControlFlowStatementKeyword, true);
        }

        private static int Fail(string message)
        {
            Console.Error.WriteLine(message);
            return 1;
        }
    }
}
