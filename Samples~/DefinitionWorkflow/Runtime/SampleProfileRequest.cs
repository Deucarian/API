using System;
namespace Deucarian.API.Samples.DefinitionWorkflow
{
    [Serializable] public sealed class SampleProfileRequest { public string Name = "Ada"; }
    [EndpointKeySet] public static class SampleEndpoints
    {
        public static EndpointKey<SampleProfileRequest, string> Profile => new Key();
        private sealed class Key : EndpointKey<SampleProfileRequest, string> { public Key() : base("sample.workflow.profile") { } }
    }
}
