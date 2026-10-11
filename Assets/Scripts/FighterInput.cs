using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// Press F to toggle the fighting stance, left mouse button to punch (jab) at any time.
/// Put this on the object that has the Animator (the "Character" model).
[RequireComponent(typeof(Animator))]
public class FighterInput : MonoBehaviour
{
    public bool fighting;
    public float jabCooldown = 0.35f;

    Animator anim;
    float nextJab;
    static readonly int FightingHash = Animator.StringToHash("Fighting");
    static readonly int JabHash = Animator.StringToHash("Jab");

    void Awake() { anim = GetComponent<Animator>(); }

    void Update()
    {
        bool togglePressed, jabPressed;
#if ENABLE_INPUT_SYSTEM
        togglePressed = Keyboard.current != null && Keyboard.current[Key.F].wasPressedThisFrame;
        jabPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        togglePressed = Input.GetKeyDown(KeyCode.F);
        jabPressed = Input.GetMouseButtonDown(0);
#endif
        if (togglePressed) fighting = !fighting;
        anim.SetBool(FightingHash, fighting);

        // left mouse button punches at any time (it also raises the guard)
        if (jabPressed && Time.time >= nextJab)
        {
            fighting = true;
            anim.SetBool(FightingHash, true);
            nextJab = Time.time + jabCooldown;
            anim.ResetTrigger(JabHash);
            anim.SetTrigger(JabHash);
        }
    }
}
