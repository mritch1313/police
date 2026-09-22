using UnityEngine;

namespace PoliceChase.Core
{
    public static class AutoBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            Debug.Log("[AutoBootstrap] Initializing core systems before scene load");

            // Create a persistent bootstrap object
            var go = new GameObject("GameBootstrap_Auto");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<GameBootstrap>();
            go.AddComponent<GameState>();
            go.AddComponent<SettingsManager>();
            go.AddComponent<SaveManager>();
            go.AddComponent<PerformanceManager>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Debug.Log("[AutoBootstrap] After scene load - ensuring player and world");
            var bootstrap = Object.FindObjectOfType<GameBootstrap>();
            if (bootstrap == null)
            {
                var go = new GameObject("GameBootstrap_Auto2");
                go.AddComponent<GameBootstrap>();
            }
        }
    }
}
