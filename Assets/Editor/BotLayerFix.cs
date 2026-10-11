using UnityEditor;
using UnityEngine;

public static class BotLayerFix
{
    // Bots must not block the player's third-person camera (same layer as the player: Ignore Raycast).
    [MenuItem("Tools/Character/Fix Bot Layers")]
    public static void Run()
    {
        foreach (var name in new[] { "Bot", "IronBot" })
        {
            var g = GameObject.Find(name);
            if (g == null) continue;
            foreach (var t in g.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = 2;
            EditorUtility.SetDirty(g);
            Debug.Log("[Bot] Layer set to Ignore Raycast on " + name);
        }
        var any = GameObject.Find("Character.001");
        if (any != null) UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(any.scene);
    }
}
