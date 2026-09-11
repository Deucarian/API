using System;
using Deucarian.Editor;
using UnityEditor;

namespace Deucarian.API.Editor
{
    [CustomPropertyDrawer(typeof(EndpointKey<,>), true)]
    public sealed class EndpointKeyDrawer : DeucarianKeyDrawer
    {
        public override Type KeyType => typeof(EndpointKey<,>);
        public override Type DefinitionSetAttribute => typeof(EndpointKeySetAttribute);
        public override string SetupHint => "Select an existing EndpointKey; declare reusable keys once in a [EndpointKeySet] class.";
    }
}
