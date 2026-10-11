using UnityEditor;
using UnityEngine;
using SUPERCharacter;
using UnityEditor.SceneManagement;

public static class IsometricSetup
{
    [MenuItem("Tools/Character/Enable Isometric View")]
    public static void Enable()
    {
        var player = GameObject.Find("Character.001");
        var main = GameObject.Find("Main Camera");
        if (player == null || main == null) { Debug.LogError("Missing Character.001 or Main Camera"); return; }
        var ctrl = player.GetComponent<SUPERCharacterAIO>();
        var pcam = ctrl.playerCamera;
        main.SetActive(true);
        var cam = main.GetComponent<Camera>();
        cam.enabled = true;
        cam.tag = "MainCamera";
        var al = main.GetComponent<AudioListener>(); if (al == null) al = main.AddComponent<AudioListener>(); al.enabled = true;
        var iso = main.GetComponent<IsometricCamera>(); if (iso == null) iso = main.AddComponent<IsometricCamera>();
        iso.target = player.transform; iso.enabled = true;
        if (pcam != null)
        {
            pcam.enabled = false;
            var pal = pcam.GetComponent<AudioListener>(); if (pal) pal.enabled = false;
            pcam.tag = "Untagged";
        }
        ctrl.enableCameraControl = false;
        ctrl.lockAndHideMouse = false;
        EditorUtility.SetDirty(ctrl); EditorUtility.SetDirty(main);
        EditorSceneManager.MarkSceneDirty(player.scene);
        Debug.Log("Isometric view enabled");
    }

    [MenuItem("Tools/Character/Disable Isometric View")]
    public static void Disable()
    {
        var player = GameObject.Find("Character.001");
        var main = GameObject.Find("Main Camera");
        var ctrl = player.GetComponent<SUPERCharacterAIO>();
        var pcam = ctrl.playerCamera;
        var iso = main.GetComponent<IsometricCamera>(); if (iso) iso.enabled = false;
        main.GetComponent<Camera>().orthographic = false;
        main.GetComponent<Camera>().enabled = false;
        var al = main.GetComponent<AudioListener>(); if (al) al.enabled = false;
        if (pcam != null) { pcam.enabled = true; pcam.tag = "MainCamera"; var pal = pcam.GetComponent<AudioListener>(); if (pal) pal.enabled = true; }
        ctrl.enableCameraControl = true; ctrl.lockAndHideMouse = true;
        EditorUtility.SetDirty(ctrl);
        EditorSceneManager.MarkSceneDirty(player.scene);
        Debug.Log("Isometric view disabled");
    }
}
