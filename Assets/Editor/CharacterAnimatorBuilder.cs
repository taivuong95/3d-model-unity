// Auto-generated: builds an Animator Controller for the SUPER Character Controller
// using Kevin Iglesias "Human Animations" clips, then assigns it to the scene character.
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class CharacterAnimatorBuilder
{
    const string Root = "Assets/Kevin Iglesias/Human Animations/Animations/Male/";
    const string OutPath = "Assets/Player/CharacterAnimator.controller";

    static AnimationClip Clip(string path)
    {
        var c = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
            .FirstOrDefault(x => !x.name.StartsWith("__preview__"));
        if (c == null) Debug.LogError("[AnimatorBuilder] Clip not found: " + path);
        return c;
    }

    static AnimatorStateTransition T(AnimatorState from, AnimatorState to, float dur = 0.15f)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false; t.duration = dur;
        return t;
    }

    [MenuItem("Tools/Character/Build Character Animator")]
    public static void Build()
    {
        var idle   = Clip(Root + "Idles/HumanM@Idle01.fbx");
        var walk   = Clip(Root + "Movement/Walk/HumanM@Walk01_Forward.fbx");
        var run    = Clip(Root + "Movement/Run/HumanM@Run01_Forward.fbx");
        var sprint = Clip(Root + "Movement/Sprint/HumanM@Sprint01_Forward.fbx");
        var fall   = Clip(Root + "Movement/Jump/HumanM@Fall01.fbx");
        var slide  = AssetDatabase.LoadAllAssetsAtPath("Assets/SUPER Character Controller/Demo Assets/FBXs/Characters/Character_Base.fbx")
                        .OfType<AnimationClip>().FirstOrDefault(c => c.name.ToLower().Contains("slid"));

        AssetDatabase.DeleteAsset(OutPath);
        var ac = AnimatorController.CreateAnimatorControllerAtPath(OutPath);
        ac.AddParameter("Vel", AnimatorControllerParameterType.Float);
        ac.AddParameter("Idle", AnimatorControllerParameterType.Bool);
        ac.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        ac.AddParameter("Sprinting", AnimatorControllerParameterType.Bool);
        ac.AddParameter("Sliding", AnimatorControllerParameterType.Bool);
        ac.AddParameter("Fighting", AnimatorControllerParameterType.Bool);
        ac.AddParameter("Jab", AnimatorControllerParameterType.Trigger);
        var ps = ac.parameters; ps.First(p => p.name == "Grounded").defaultBool = true; ac.parameters = ps;

        var sm = ac.layers[0].stateMachine;

        var sIdle = sm.AddState("Idle", new Vector3(300, 0));
        sIdle.motion = idle;

        // Locomotion blend tree: Vel (m/s) -> idle / walk / run
        var sMove = ac.CreateBlendTreeInController("Locomotion", out BlendTree bt, 0);
        bt.blendParameter = "Vel"; bt.useAutomaticThresholds = false;
        bt.AddChild(idle, 0f); bt.AddChild(walk, 2f); bt.AddChild(run, 5f);

        var sSprint = sm.AddState("Sprint", new Vector3(560, 120)); sSprint.motion = sprint;
        var sAir = sm.AddState("AirBorn", new Vector3(300, -120)); sAir.motion = fall;
        AnimatorState sSlide = null;
        if (slide != null) { sSlide = sm.AddState("Slide", new Vector3(560, 240)); sSlide.motion = slide; }
        sm.defaultState = sIdle;

        T(sIdle, sMove).AddCondition(AnimatorConditionMode.Greater, 0.1f, "Vel");
        T(sMove, sIdle).AddCondition(AnimatorConditionMode.Less, 0.1f, "Vel");
        T(sMove, sSprint).AddCondition(AnimatorConditionMode.If, 0, "Sprinting");
        var t = T(sSprint, sMove); t.AddCondition(AnimatorConditionMode.IfNot, 0, "Sprinting");
        T(sSprint, sIdle).AddCondition(AnimatorConditionMode.Less, 0.1f, "Vel");

        var toAir = sm.AddAnyStateTransition(sAir);
        toAir.hasExitTime = false; toAir.duration = 0.2f; toAir.canTransitionToSelf = false;
        toAir.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");
        t = T(sAir, sIdle, 0.1f); t.AddCondition(AnimatorConditionMode.If, 0, "Grounded"); t.AddCondition(AnimatorConditionMode.Less, 0.1f, "Vel");
        t = T(sAir, sMove, 0.1f); t.AddCondition(AnimatorConditionMode.If, 0, "Grounded"); t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Vel");

        if (sSlide != null)
        {
            var toSlide = sm.AddAnyStateTransition(sSlide);
            toSlide.hasExitTime = false; toSlide.duration = 0.15f; toSlide.canTransitionToSelf = false;
            toSlide.AddCondition(AnimatorConditionMode.If, 0, "Sliding");
            t = T(sSlide, sMove); t.AddCondition(AnimatorConditionMode.IfNot, 0, "Sliding");
        }

        // ---- Combat: Boxing stance (loop) + Lead Jab ----
        var boxing = Clip("Assets/Animations/Boxing.fbx");
        var jab = Clip("Assets/Animations/Lead Jab.fbx");
        if (boxing != null && jab != null)
        {
            var sFight = sm.AddState("Fight Stance", new Vector3(300, -240)); sFight.motion = boxing;
            var sJab = sm.AddState("Jab", new Vector3(560, -240)); sJab.motion = jab; sJab.speed = 1.3f;

            T(sIdle, sFight, 0.2f).AddCondition(AnimatorConditionMode.If, 0, "Fighting");
            t = T(sFight, sIdle, 0.2f); t.AddCondition(AnimatorConditionMode.IfNot, 0, "Fighting");
            t = T(sFight, sMove, 0.15f); t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Vel");
            t = T(sMove, sFight, 0.15f); t.AddCondition(AnimatorConditionMode.If, 0, "Fighting"); t.AddCondition(AnimatorConditionMode.Less, 0.1f, "Vel");
            T(sFight, sJab, 0.05f).AddCondition(AnimatorConditionMode.If, 0, "Jab");
            T(sIdle, sJab, 0.05f).AddCondition(AnimatorConditionMode.If, 0, "Jab");
            T(sMove, sJab, 0.08f).AddCondition(AnimatorConditionMode.If, 0, "Jab");
            var back = sJab.AddTransition(sFight);
            back.hasExitTime = true; back.exitTime = 0.85f; back.duration = 0.1f;
        }
        else Debug.LogWarning("[AnimatorBuilder] Combat clips not found; run Tools/Character/Setup Combat Animations first.");

        AssetDatabase.SaveAssets();

        // Assign to the character's Animator
        var go = GameObject.Find("Character.001/Character");
        var anim = go ? go.GetComponent<Animator>() : null;
        if (anim != null)
        {
            Undo.RecordObject(anim, "Assign Animator Controller");
            anim.runtimeAnimatorController = ac;
            anim.applyRootMotion = false;
            EditorUtility.SetDirty(anim);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
            Debug.Log("[AnimatorBuilder] Done. Controller assigned to " + go.name + " (slide clip: " + (slide ? slide.name : "none") + ")");
        }
        else Debug.LogWarning("[AnimatorBuilder] Controller created at " + OutPath + " but Character.001/Character not found.");
    }
}
