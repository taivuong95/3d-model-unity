using UnityEditor;
using UnityEngine;

public static class BotSetup
{
    static Texture2D HueShift(string srcAssetPath, string dstAssetPath, float shift)
    {
        string root = System.IO.Directory.GetParent(Application.dataPath).FullName;
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!tex.LoadImage(System.IO.File.ReadAllBytes(System.IO.Path.Combine(root, srcAssetPath)))) return null;
        var px = tex.GetPixels32();
        for (int i = 0; i < px.Length; i++)
        {
            Color.RGBToHSV(px[i], out float h, out float sat, out float v);
            if (sat > 0.5f && v > 0.12f)
            {
                h = Mathf.Repeat(h + shift, 1f);
                var c = (Color32)Color.HSVToRGB(h, sat, v);
                c.a = px[i].a; px[i] = c;
            }
        }
        tex.SetPixels32(px); tex.Apply();
        System.IO.File.WriteAllBytes(System.IO.Path.Combine(root, dstAssetPath), tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(dstAssetPath);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(dstAssetPath);
    }

    [MenuItem("Tools/Character/Create Bot")]
    public static void Create()
    {
        var player = GameObject.Find("Character.001");
        var srcModel = GameObject.Find("Character.001/Character");
        if (player == null || srcModel == null) { Debug.LogError("[Bot] Player or model not found."); return; }

        var old = GameObject.Find("Bot");
        if (old != null) Object.DestroyImmediate(old);

        // spawn 4m diagonally from the player, on the terrain
        Vector3 p = player.transform.position + new Vector3(4f, 0f, 4f);
        var terrain = Terrain.activeTerrain;
        p.y = terrain != null ? terrain.SampleHeight(p) + terrain.transform.position.y + 0.05f : player.transform.position.y;

        var root = new GameObject("Bot");
        root.transform.position = p;
        var cc = root.AddComponent<CharacterController>();
        cc.height = 1.75f; cc.radius = 0.3f; cc.center = new Vector3(0f, 0.875f, 0f);
        cc.slopeLimit = 55f; cc.stepOffset = 0.4f;

        var model = Object.Instantiate(srcModel, root.transform);
        model.name = "Model";
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;
        var fi = model.GetComponent<FighterInput>();
        if (fi != null) Object.DestroyImmediate(fi);   // the bot must not react to player input

        // red-tinted copy of the material(s)
        foreach (var r in model.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                string src = AssetDatabase.GetAssetPath(mats[i]);
                string dst = "Assets/Player/BotMaterial_" + i + ".mat";
                AssetDatabase.DeleteAsset(dst);
                if (!AssetDatabase.CopyAsset(src, dst)) { Debug.LogWarning("[Bot] Could not copy material " + src); continue; }
                var m = AssetDatabase.LoadAssetAtPath<Material>(dst);
                // Real colour change: shift the hue of strongly saturated pixels (the red dress) to blue,
                // leaving low-saturation pixels (skin, hair) mostly untouched.
                var baseTex = m.HasProperty("_BaseMap") ? m.GetTexture("_BaseMap") as Texture2D : null;
                if (baseTex != null)
                {
                    string tp = AssetDatabase.GetAssetPath(baseTex);
                    var shifted = HueShift(tp, "Assets/Player/texture_diffuse_bot.png", 0.58f);
                    if (shifted != null) m.SetTexture("_BaseMap", shifted);
                }
                else
                {
                    var tint = new Color(0.3f, 0.45f, 1f, 1f);
                    if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", tint);
                }
                EditorUtility.SetDirty(m);
                mats[i] = m;
            }
            r.sharedMaterials = mats;
        }
        AssetDatabase.SaveAssets();

        var ai = root.AddComponent<BotAI>();
        ai.target = player.transform;
        ai.animator = model.GetComponent<Animator>();

        foreach (var tr in root.GetComponentsInChildren<Transform>(true)) tr.gameObject.layer = 2;  // do not block the camera
        Selection.activeGameObject = root;
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
        Debug.Log("[Bot] Created at " + p + " (materials tinted red)");
    }
}
