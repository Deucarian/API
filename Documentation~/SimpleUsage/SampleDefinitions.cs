namespace Deucarian.API.Samples.SimpleUsage
{
    [EndpointKeySet]
    public static class Endpoints
    {
        public static EndpointKey<ProfileRequest, ProfileResponse> Profile => new Definition();
        private sealed class Definition : EndpointKey<ProfileRequest, ProfileResponse>
        {
            public Definition() : base("sample.profile") { }
        }
    }
    public sealed class ProfileRequest { }
    public sealed class ProfileResponse { public string DisplayName; }
}
