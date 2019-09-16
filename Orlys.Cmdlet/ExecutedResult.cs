
namespace Orlys.Cmdlet
{
    using System;

    public sealed class ExecutedResult
    {
        internal ExecutedResult(object @object)
        {
            this.Value =  @object;
        }

        internal ExecutedResult(Exception e)
        {
            this.Error = e;
        }

        public Exception Error { get; }

        public bool IsSuccess => this.Error == null;

        public object Value { get; }

        public static implicit operator bool(ExecutedResult result)
        {
            return result.IsSuccess;
        }
        public override string ToString() => this.IsSuccess.ToString();
    }

}
