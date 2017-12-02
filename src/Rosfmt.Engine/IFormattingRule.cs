using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Rosfmt
{
    public interface IFormattingRule
    {
        Task<ImmutableArray<Diagnostic>> EvaluateAsync(Document document);
    }
}
