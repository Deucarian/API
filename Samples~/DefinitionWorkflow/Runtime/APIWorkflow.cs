using System;
using UnityEngine;
using Deucarian.API.Core;
namespace Deucarian.API.Samples.DefinitionWorkflow
{
    /// <summary>Small caller example. The configured scene hosts own services and resource lifetimes.</summary>
    public sealed class APIWorkflow : MonoBehaviour
    {
        [SerializeField] private ApiHost host;
        [SerializeField] private EndpointKey<SampleProfileRequest, string> endpoint = SampleEndpoints.Profile;
        [SerializeField] private SampleProfileTrigger trigger;
        private string status = "Ready. Choose an action below.";
        public string Status => status;
        public async void Send() { var result = await host.SendAsync(endpoint, new SampleProfileRequest()); status = result.IsSuccess ? result.Data : result.ErrorMessage; }
        public void SendComponent() { trigger.Send(); status = "Request sent through the typed component. The sample transport is local."; }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(24, 24, Math.Min(540, Screen.width - 48), Screen.height - 48), GUI.skin.box);
            GUILayout.Label("API — definition workflow");
            GUILayout.Label("One startup component registers the typed endpoint and a local mock transport. Callers and Inspector components reuse the same request and response contract.");
            GUILayout.Space(12);
            if (GUILayout.Button("Send typed request", GUILayout.Height(32))) { try { Send(); } catch (Exception error) { status = error.Message; } }
            if (GUILayout.Button("Send from component", GUILayout.Height(32))) { try { SendComponent(); } catch (Exception error) { status = error.Message; } }
            GUILayout.Space(12);
            GUILayout.Label(status);
            GUILayout.EndArea();
        }
    }
}
