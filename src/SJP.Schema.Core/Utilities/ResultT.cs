namespace SJP.Schema.Core.Utilities
{
    public class Result<TValue> : IResult<TValue>
    {
        public Result(bool success, TValue value)
        {
            Success = success;
            Value = value;
        }

        public bool Success { get; }

        public TValue Value { get; }
    }
}
