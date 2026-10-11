using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CombatSetup
{
    static readonly string[] Files = { "Boxing", "Lead Jab" };

    [MenuItem("Tools/Character/Setup Combat Animations")]
    public static void Run()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Animations")) AssetDatabase.CreateFolder("Assets", "Animations");

        foreach (var name in Files)
        {
            string path = "Assets/Animations/" + name + ".fbx";
            string old = "Assets/" + name + ".fbx";
            if (!System.IO.File.Exists(path.Replace("Assets", Application.dataPath)) && System.IO.File.Exists(old.Replace("Assets", Application.dataPath)))
            {
                var err = AssetDatabase.MoveAsset(old, path);
                if (!string.IsNullOrEmpty(err)) { Debug.LogError("[Combat] Move failed: " + err); return; }
            }

            var imp = AssetImporter.GetAtPath(path) as ModelImporter;
            if (imp == null) { Debug.LogError("[Combat] Not found: " + path); return; }

            imp.animationType = ModelImporterAnimationType.Human;
            imp.sourceAvatar = null;
            imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel; // own humanoid avatar; Humanoid retargets to our character
            imp.SaveAndReimport();

            imp = AssetImporter.GetAtPath(path) as ModelImporter;
            var clips = imp.defaultClipAnimations;
            foreach (var c in clips)
            {
                c.name = name;
                c.loopTime = (name == "Boxing");
                c.loopPose = (name == "Boxing");
                // keep the animation "in place": the capsule moves the character, not the clip
                c.lockRootRotation = true; c.lockRootHeightY = true; c.lockRootPositionXZ = true;
                c.keepOriginalOrientation = true; c.keepOriginalPositionY = true; c.keepOriginalPositionXZ = true;
            }
            imp.clipAnimations = clips;
            imp.SaveAndReimport();
            Debug.Log("[Combat] Imported as Humanoid: " + path);
        }

        CharacterAnimatorBuilder.Build();

        var go = GameObject.Find("Character.001/Character");
        if (go != null && go.GetComponent<FighterInput>() == null)
        {
            ObjectFactory.AddComponent<FighterInput>(go);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
            Debug.Log("[Combat] Added FighterInput to " + go.name);
        }
    }
}
