
using UnityEngine;
using UnityEngine.InputSystem; // New Input System (Keyboard.current etc.)

// yaha we are ensuring this GameObject, must have a Rigidbody2D.
// agar koi bhul jaye toh Unity will add it automatically, aur error nahi aayega.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    // ye speed of movement hai (units per second-ish)
    [SerializeField] private float moveSpeed = 6f;

    [SerializeField] private Animator animator;


    private Rigidbody2D rb;        // Physis body component, jisme hum movement apply karenge
    private Vector2 moveInput;     // vector de jo keyboard se input store karega, x aur y dono ke liye

    private Vector2 lastDir = Vector2.down;
    private void Awake()
    {
        // rigidbody component ko get karo, aur store karlo variable me for later use
        rb = GetComponent<Rigidbody2D>();

        // top down game me gravity nahi chahiye, toh gravity scale ko zero kar diya
        rb.gravityScale = 0f;

        // colliders ke saath physics interactions me rotation nahi chahiye, toh freezeRotation true.
        rb.freezeRotation = true;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // har frame mai ye function chalta hai, aur yaha hum keyboard input read karenge.

        Vector2 input = Vector2.zero;

        // keyboard ko grab karo, kuch systems pe ho sakta hai there is no keyboard, toh null checking
        var k = Keyboard.current;
        if (k == null) return;

        // Left and Right ka logic
        if (k.aKey.isPressed || k.leftArrowKey.isPressed) input.x -= 1f;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) input.x += 1f;

        // Down and Up ka logic
        if (k.sKey.isPressed || k.downArrowKey.isPressed) input.y -= 1f;
        if (k.wKey.isPressed || k.upArrowKey.isPressed) input.y += 1f;


        // normalise karne se diagonal movement bhi same speed pe hota hai, warna diagonal me speed zyada ho jati hai.
        moveInput = input.normalized;

        if (animator != null)
        {
            float speed = moveInput.magnitude;
            animator.SetFloat("Speed", speed);

            if (speed > 0.001f)
            {
                // snap so you always land on one of the 8 directions (no mushy blending)
                Vector2 dir = SnapTo8(moveInput);

                animator.SetFloat("MoveX", dir.x);
                animator.SetFloat("MoveY", dir.y);

                // keep last facing for idle
                lastDir = dir;
                animator.SetFloat("LastMoveX", lastDir.x);
                animator.SetFloat("LastMoveY", lastDir.y);
            }
        }
        
    }

    private void FixedUpdate()
    {
        // fixed update ko physics update ke liye use karte hai, and ye har tick me chalta hai, frame rate se independent.
        // ya pe hum rigidbody ko directly move karenge, toh smooth movement milega.

        rb.linearVelocity = moveInput * moveSpeed;
    }

    private Vector2 SnapTo8(Vector2 v)
    {
        if (v == Vector2.zero) return Vector2.down;

        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        int slice = Mathf.RoundToInt(angle / 45f) % 8;

        switch (slice)
        {
            case 0: return Vector2.right;                  // E
            case 1: return new Vector2(1, 1).normalized;   // NE
            case 2: return Vector2.up;                     // N
            case 3: return new Vector2(-1, 1).normalized;  // NW
            case 4: return Vector2.left;                   // W
            case 5: return new Vector2(-1, -1).normalized; // SW
            case 6: return Vector2.down;                   // S
            case 7: return new Vector2(1, -1).normalized;  // SE
        }

        return Vector2.down;
    }
}