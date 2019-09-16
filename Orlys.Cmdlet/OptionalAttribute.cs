
namespace Orlys.Cmdlet
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class OptionalAttribute : Attribute
    {
        public string Name;
    }

}
