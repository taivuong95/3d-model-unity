using System.Reflection;
using UnityEngine;
using SUPERCharacter;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Camera isometric (orthographic) bam theo nhan vat. W/A/S/D di chuyen theo huong man hinh.
[DefaultExecutionOrder(-1000)]
public class IsometricCamera : MonoBehaviour
{
    public Transform target;
    public float yaw = 45f;
    public float pitch = 35.264f;
    public float distance = 40f;
    public float orthoSize = 6f;
    public float minSize = 3f, maxSize = 16f, zoomSpeed = 1.5f;
    public float followSmooth = 12f;
    public Vector3 targetOffset = new Vector3(0, 1f, 0);

    SUPERCharacterAIO player;
    FieldInfo headRotField, headDirField;
    Camera cam;
    Vector3 pos;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (target) player = target.GetComponent<SUPERCharacterAIO>();
        var t = typeof(SUPERCharacterAIO);
        var f = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        headRotField = t.GetField("headRot", f);
        headDirField = t.GetField("HeadRotDirForInput", f);
        if (cam) { cam.orthographic = true; cam.orthographicSize = orthoSize; cam.nearClipPlane = 0.1f; cam.farClipPlane = 300f; }
        if (target) pos = target.position + targetOffset;
    }

    void ForceYaw()
    {
        if (player == null) return;
        if (headRotField != null) headRotField.SetValue(player, new Vector3(pitch, yaw, 0));
        if (headDirField != null) headDirField.SetValue(player, yaw);
    }

    void FixedUpdate() { ForceYaw(); }

    void Update()
    {
        ForceYaw();
        float scroll = 0;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) scroll = Mouse.current.scroll.ReadValue().y / 120f;
#else
        scroll = Input.mouseScrollDelta.y;
#endif
        if (cam && scroll != 0)
        {
            orthoSize = Mathf.Clamp(orthoSize - scroll * zoomSpeed, minSize, maxSize);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 goal = target.position + targetOffset;
        pos = Vector3.Lerp(pos, goal, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
        transform.rotation = rot;
        transform.position = pos - rot * Vector3.forward * distance;
        if (cam) cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, orthoSize, 1f - Mathf.Exp(-10f * Time.deltaTime));
    }
}
