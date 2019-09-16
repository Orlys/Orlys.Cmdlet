
namespace Orlys.Cmdlet
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MarkAttribute : Attribute
    {
        public string Name;

        internal int Index { get; set; }
    }

}
