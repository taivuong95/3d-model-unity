using System.Linq;
using UnityEditor;
using UnityEngine;

/// Makes the Meshy "Iron Resolve" model the player-controlled character.
public static class IronPlayerSetup
{
    const string Dir = "Assets/Enemy/IronResolve/";
    const string FbxPath = Dir + "IronResolve.fbx";
    const string MatPath = Dir + "IronResolve.mat";

    static void SetTex(string path, System.Action<TextureImporter> f)
    {
        var t = AssetImporter.GetAtPath(path) as TextureImporter;
        if (t == null) { Debug.LogWarning("[IronPlayer] Texture missing: " + path); return; }
        f(t); t.SaveAndReimport();
    }

    [MenuItem("Tools/Character/Make Iron Resolve The Player")]
    public static void Run()
    {
        var player = GameObject.Find("Character.001");
        if (player == null) { Debug.LogError("[IronPlayer] 'Character.001' not found."); return; }

        // ---- 1) textures + material ----
        SetTex(Dir + "IronResolve_normal.png", t => t.textureType = TextureImporterType.NormalMap);
        SetTex(Dir + "IronResolve_metal_smooth.png", t => { t.sRGBTexture = false; t.alphaSource = TextureImporterAlphaSource.FromInput; t.alphaIsTransparency = false; });
        SetTex(Dir + "IronResolve_basecolor.png", t => t.sRGBTexture = true);

        AssetDatabase.DeleteAsset(MatPath);
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "IronResolve" };
        mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Dir + "IronResolve_basecolor.png"));
        mat.SetColor("_BaseColor", Color.white);
        mat.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Dir + "IronResolve_normal.png"));
        mat.EnableKeyword("_NORMALMAP");
        mat.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Dir + "IronResolve_metal_smooth.png"));
        mat.EnableKeyword("_METALLICSPECGLOSSMAP");
        mat.SetFloat("_Smoothness", 1f);
        AssetDatabase.CreateAsset(mat, MatPath);
        AssetDatabase.SaveAssets();

        // ---- 2) remove the IronBot copy (the model becomes the player instead) ----
        var ironBot = GameObject.Find("IronBot");
        if (ironBot != null) Object.DestroyImmediate(ironBot);

        // ---- 3) hide the old model, add the Iron Resolve model under the player ----
        var oldModel = player.transform.Find("Character");
        if (oldModel != null && oldModel.GetComponent<Animator>() != null && !oldModel.name.Contains("Iron"))
        {
            oldModel.name = "Character (Red, disabled)";
            oldModel.gameObject.SetActive(false);
        }

        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        var model = (GameObject)PrefabUtility.InstantiatePrefab(fbx, player.transform);
        model.name = "Character";           // same name so existing lookups keep working
        model.transform.localPosition = new Vector3(0f, -0.875f, 0f);
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        foreach (var r in model.GetComponentsInChildren<Renderer>())
        {
            var mats = new Material[Mathf.Max(1, r.sharedMaterials.Length)];
            for (int i = 0; i < mats.Length; i++) mats[i] = mat;
            r.sharedMaterials = mats;
        }

        var rs = model.GetComponentsInChildren<Renderer>();
        var b = rs[0].bounds; for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        Debug.Log("[IronPlayer] model height " + b.size.y.ToString("F3") + ", feet offset from pivot " + (b.min.y - model.transform.position.y).ToString("F3"));

        var anim = model.GetComponent<Animator>() ?? model.AddComponent<Animator>();
        anim.applyRootMotion = false;

        // ---- 4) tell the SUPER controller to drive the new Animator ----
        var aio = player.GetComponent("SUPERCharacterAIO");
        if (aio != null)
        {
            var so = new SerializedObject(aio);
            var prop = so.FindProperty("_3rdPersonCharacterAnimator");
            if (prop != null) { prop.objectReferenceValue = anim; so.ApplyModifiedProperties(); Debug.Log("[IronPlayer] SUPER controller now drives the Iron Resolve Animator."); }
            else Debug.LogWarning("[IronPlayer] Field _3rdPersonCharacterAnimator not found.");
        }

        // ---- 5) input script + controller ----
        if (model.GetComponent<FighterInput>() == null) model.AddComponent<FighterInput>();
        CharacterAnimatorBuilder.Build();   // rebuilds the controller and assigns it to Character.001/Character

        // the rebuild recreates the controller asset, so re-link the other bot that uses it
        var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Player/CharacterAnimator.controller");
        foreach (var botName in new[] { "Bot/Model" })
        {
            var g = GameObject.Find(botName);
            var a = g != null ? g.GetComponent<Animator>() : null;
            if (a != null) a.runtimeAnimatorController = ctrl;
        }

        EditorUtility.SetDirty(player);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(player.scene);
        Debug.Log("[IronPlayer] Done.");
    }
}
