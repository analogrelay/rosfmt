using System.Threading.Tasks;
using Rosfmt.Rules;
using Xunit;

namespace Rosfmt.Engine.Tests.Rules
{
    public class UsingsMustBeSortedRuleTests
    {
        public class NormalSorting
        {
            [Fact]
            public async Task ReportedInCompilationUnit()
            {
                const string code = @"
[|using System.Text;
using Alpha;|]
";

                await RuleTest.SingleErrorTestAsync(new UsingsMustBeSortedRule(sortSystemFirst: false), code, UsingsMustBeSortedRule.DiagnosticDescriptor);
            }

            [Fact]
            public async Task ReportedInNamespace()
            {
                const string code = @"
namespace Foo {
    [|using System.Text;
    using Alpha;|]
}
";

                await RuleTest.SingleErrorTestAsync(new UsingsMustBeSortedRule(sortSystemFirst: false), code, UsingsMustBeSortedRule.DiagnosticDescriptor);
            }
        }

        public class SystemFirstSorting
        {
            [Fact]
            public async Task ReportedInCompilationUnit()
            {
                const string code = @"
[|using Alpha;
using System.Collections;|]
";

                await RuleTest.SingleErrorTestAsync(new UsingsMustBeSortedRule(sortSystemFirst: true), code, UsingsMustBeSortedRule.DiagnosticDescriptor);
            }

            [Fact]
            public async Task ReportedInNamespace()
            {
                const string code = @"
namespace Foo {
    [|using Alpha;
    using System.Collections;|]
}
";

                await RuleTest.SingleErrorTestAsync(new UsingsMustBeSortedRule(sortSystemFirst: true), code, UsingsMustBeSortedRule.DiagnosticDescriptor);
            }
        }
    }
}
