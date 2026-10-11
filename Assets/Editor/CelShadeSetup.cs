using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public static class CelShadeSetup
{
    const string MatPath = "Assets/Enemy/IronResolve/IronResolveToon.mat";
    const string OrigPath = "Assets/Enemy/IronResolve/IronResolve.mat";

    static void Assign(Material m)
    {
        var model = GameObject.Find("Character.001/Character");
        if (model == null) { Debug.LogError("Character model not found"); return; }
        foreach (var r in model.GetComponentsInChildren<Renderer>(true))
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++) mats[i] = m;
            r.sharedMaterials = mats;
            EditorUtility.SetDirty(r);
        }
        EditorSceneManager.MarkSceneDirty(model.scene);
    }

    [MenuItem("Tools/Character/Apply Cel Shading")]
    public static void Apply()
    {
        AssetDatabase.Refresh();
        var shader = Shader.Find("Custom/ToonCel");
        if (shader == null) { Debug.LogError("Shader Custom/ToonCel not found / compile error"); return; }
        var m = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
        if (m == null) { m = new Material(shader); AssetDatabase.CreateAsset(m, MatPath); }
        m.shader = shader;
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Enemy/IronResolve/IronResolve_basecolor.png");
        m.SetTexture("_BaseMap", tex);
        EditorUtility.SetDirty(m);
        AssetDatabase.SaveAssets();
        Assign(m);
        Debug.Log("Cel shading applied");
    }

    [MenuItem("Tools/Character/Remove Cel Shading")]
    public static void Remove()
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(OrigPath);
        if (m != null) Assign(m);
        Debug.Log("Cel shading removed");
    }
}
