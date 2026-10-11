using UnityEngine;

/// Simple fighter bot: chases the target, raises its guard when close, jabs when in range.
/// Uses the same Animator Controller / parameters as the player (Vel, Idle, Grounded, Fighting, Jab).
[RequireComponent(typeof(CharacterController))]
public class BotAI : MonoBehaviour
{
    public Transform target;
    public Animator animator;

    [Header("Ranges (metres)")]
    public float aggroRange = 30f;     // starts chasing inside this distance
    public float fightRange = 6f;      // raises guard (Boxing stance) inside this distance
    public float attackRange = 1.5f;   // stops and jabs inside this distance

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float acceleration = 12f;
    public float turnSpeed = 540f;     // degrees / second
    public float gravity = -20f;

    [Header("Attack")]
    public float attackCooldown = 1.4f;
    public float settleTime = 0.6f;    // must stand still this long (in guard) before a jab

    CharacterController cc;
    float speed, vy, nextAttack, stillTimer, airTimer;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        Vector3 to = Vector3.zero;
        float dist = float.MaxValue;
        if (target != null)
        {
            to = target.position - transform.position; to.y = 0f;
            dist = to.magnitude;
        }

        bool chase = target != null && dist < aggroRange && dist > attackRange * 0.9f;
        float desired = chase ? moveSpeed : 0f;
        speed = Mathf.MoveTowards(speed, desired, acceleration * dt);

        // turn toward the target
        if (target != null && dist < aggroRange && to.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(to.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * dt);
        }

        // gravity + move
        if (cc.isGrounded && vy < 0f) vy = -2f;
        vy += gravity * dt;
        Vector3 move = transform.forward * speed + Vector3.up * vy;
        cc.Move(move * dt);

        airTimer = cc.isGrounded ? 0f : airTimer + dt;
        bool grounded = airTimer < 0.15f;

        if (animator != null)
        {
            animator.SetFloat("Vel", speed);
            animator.SetBool("Idle", speed < 0.1f);
            animator.SetBool("Grounded", grounded);
            animator.SetBool("Sprinting", false);
            animator.SetBool("Sliding", false);
            animator.SetBool("Fighting", target != null && dist < fightRange);

            // attack
            bool inRange = target != null && dist <= attackRange;
            float angle = target != null ? Vector3.Angle(transform.forward, to) : 180f;
            stillTimer = (inRange && speed < 0.1f) ? stillTimer + dt : 0f;
            if (inRange && angle < 30f && stillTimer >= settleTime && Time.time >= nextAttack)
            {
                nextAttack = Time.time + attackCooldown;
                animator.ResetTrigger("Jab");
                animator.SetTrigger("Jab");
            }
        }
    }
}
