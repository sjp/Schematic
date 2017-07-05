using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schema.Core.Utilities
{
    public interface IResult<TValue>
    {
        bool Success { get; }

        TValue Value { get; }
    }
}
