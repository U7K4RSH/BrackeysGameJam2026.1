
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
    private bool invertMovement = false;

    private Vector2 lastDir = Vector2.down;
    private void Awake()
    {
        // rigidbody component ko get karo, aur store karlo variable me for later use
        rb = GetComponent<Rigidbody2D>();

       
        rb.gravityScale = 0f;

       
        rb.freezeRotation = true;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            moveInput = Vector2.zero;

            if (animator != null)
                animator.SetFloat("Speed", 0f);

            return;
        }

        Vector2 input = Vector2.zero;

        
        var k = Keyboard.current;
        if (k == null) return;

       
        if (k.aKey.isPressed || k.leftArrowKey.isPressed) input.x -= 1f;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) input.x += 1f;

        
        if (k.sKey.isPressed || k.downArrowKey.isPressed) input.y -= 1f;
        if (k.wKey.isPressed || k.upArrowKey.isPressed) input.y += 1f;


        
        Vector2 effectiveInput = invertMovement ? -input : input;
        moveInput = effectiveInput.normalized;

        if (animator != null)
        {
            float speed = moveInput.magnitude;
            animator.SetFloat("Speed", speed);

            if (speed > 0.001f)
            {
                
                Vector2 dir = SnapTo8(moveInput);

                animator.SetFloat("MoveX", dir.x);
                animator.SetFloat("MoveY", dir.y);

                
                lastDir = dir;
                animator.SetFloat("LastMoveX", lastDir.x);
                animator.SetFloat("LastMoveY", lastDir.y);
            }
        }
        
    }

    public void SetInverted(bool inverted)
    {
        invertMovement = inverted;
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
            case 0: return Vector2.right;                  
            case 1: return new Vector2(1, 1).normalized;   
            case 2: return Vector2.up;                     
            case 3: return new Vector2(-1, 1).normalized;  
            case 4: return Vector2.left;                   
            case 5: return new Vector2(-1, -1).normalized; 
            case 6: return Vector2.down;                   
            case 7: return new Vector2(1, -1).normalized;  
        }

        return Vector2.down;
    }
}