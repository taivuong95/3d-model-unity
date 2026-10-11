using System.Linq;
using UnityEditor;
using UnityEngine;

/// Builds a second bot ("IronBot") from the Meshy "Iron Resolve" FBX.
public static class IronBotSetup
{
    const string Dir = "Assets/Enemy/IronResolve/";
    const string FbxPath = Dir + "IronResolve.fbx";
    const string BasePath = Dir + "IronResolve_basecolor.png";
    const string NormalPath = Dir + "IronResolve_normal.png";
    const string MatPath = Dir + "IronResolve.mat";
    const string ControllerPath = "Assets/Player/CharacterAnimator.controller";
    const float TargetHeight = 1.75f;

    [MenuItem("Tools/Character/Create Iron Resolve Bot")]
    public static void Create()
    {
        var player = GameObject.Find("Character.001");
        if (player == null) { Debug.LogError("[IronBot] Player 'Character.001' not found."); return; }

        // 1) textures: normal map must be imported as a normal map
        var nImp = AssetImporter.GetAtPath(NormalPath) as TextureImporter;
        if (nImp != null && nImp.textureType != TextureImporterType.NormalMap)
        { nImp.textureType = TextureImporterType.NormalMap; nImp.SaveAndReimport(); }

        // 2) model: Humanoid rig (own avatar), no embedded materials, no animation clips
        var mImp = AssetImporter.GetAtPath(FbxPath) as ModelImporter;
        if (mImp == null) { Debug.LogError("[IronBot] FBX not found at " + FbxPath); return; }
        mImp.animationType = ModelImporterAnimationType.Human;
        mImp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        mImp.importAnimation = false;
        mImp.materialImportMode = ModelImporterMaterialImportMode.None;
        mImp.SaveAndReimport();

        var avatar = AssetDatabase.LoadAllAssetsAtPath(FbxPath).OfType<Avatar>().FirstOrDefault();
        Debug.Log("[IronBot] Avatar: " + (avatar == null ? "NONE" : (avatar.isValid ? "valid" : "INVALID") + ", human=" + avatar.isHuman));

        // 3) material (URP Lit): base colour + normal map
        AssetDatabase.DeleteAsset(MatPath);
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "IronResolve" };
        mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(BasePath));
        mat.SetColor("_BaseColor", Color.white);
        mat.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(NormalPath));
        mat.EnableKeyword("_NORMALMAP");
        mat.SetFloat("_Metallic", 0.5f);
        mat.SetFloat("_Smoothness", 0.45f);
        AssetDatabase.CreateAsset(mat, MatPath);
        AssetDatabase.SaveAssets();

        // 4) bot object
        var old = GameObject.Find("IronBot");
        if (old != null) Object.DestroyImmediate(old);

        Vector3 p = player.transform.position + new Vector3(2f, 0f, 6f);
        var terrain = Terrain.activeTerrain;
        p.y = terrain != null ? terrain.SampleHeight(p) + terrain.transform.position.y + 0.05f : player.transform.position.y;

        var root = new GameObject("IronBot");
        root.transform.position = p;
        var cc = root.AddComponent<CharacterController>();
        cc.height = TargetHeight; cc.radius = 0.3f; cc.center = new Vector3(0f, TargetHeight / 2f, 0f);
        cc.slopeLimit = 55f; cc.stepOffset = 0.4f;

        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        var model = (GameObject)PrefabUtility.InstantiatePrefab(fbx, root.transform);
        model.name = "Model";
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        foreach (var r in model.GetComponentsInChildren<Renderer>())
        {
            var mats = new Material[Mathf.Max(1, r.sharedMaterials.Length)];
            for (int i = 0; i < mats.Length; i++) mats[i] = mat;
            r.sharedMaterials = mats;
        }

        // scale to a human height and put the feet on the ground
        Bounds b = Bounds(model);
        float h = b.size.y;
        float k = h > 0.001f ? TargetHeight / h : 1f;
        model.transform.localScale = Vector3.one * k;
        b = Bounds(model);
        model.transform.position += Vector3.up * (root.transform.position.y - b.min.y);
        Debug.Log("[IronBot] Native height " + h.ToString("F3") + " -> scale " + k.ToString("F4"));

        var anim = model.GetComponent<Animator>() ?? model.AddComponent<Animator>();
        if (anim.avatar == null) anim.avatar = avatar;
        anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
        anim.applyRootMotion = false;

        var ai = root.AddComponent<BotAI>();
        ai.target = player.transform;
        ai.animator = anim;

        foreach (var tr in root.GetComponentsInChildren<Transform>(true)) tr.gameObject.layer = 2;  // do not block the camera
        Selection.activeGameObject = root;
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
        Debug.Log("[IronBot] Created at " + p);
    }

    static Bounds Bounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>();
        var b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        return b;
    }
}
