using System;

namespace Deucarian.API
{
    /// <summary>Marks an authoritative set of named EndpointKey fields or properties for the Inspector.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class EndpointKeySetAttribute : Attribute { }
}
