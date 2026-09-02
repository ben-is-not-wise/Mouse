using System;

namespace HackedDesign
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class TransitionsToAttribute : Attribute
    {
        public Type[] States { get; }

        public TransitionsToAttribute(params Type[] states)
        {
            States = states ?? Array.Empty<Type>();
        }
    }
}
