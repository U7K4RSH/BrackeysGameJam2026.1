
using UnityEngine;
using UnityEngine.InputSystem; // New Input System (Keyboard.current etc.)

// yaha we are ensuring this GameObject, must have a Rigidbody2D.
// agar koi bhul jaye toh Unity will add it automatically, aur error nahi aayega.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    // ye speed of movement hai (units per second-ish)
    [SerializeField] private float moveSpeed = 6f;

    private Rigidbody2D rb;        // Physis body component, jisme hum movement apply karenge
    private Vector2 moveInput;     // vector de jo keyboard se input store karega, x aur y dono ke liye

    private void Awake()
    {
        // rigidbody component ko get karo, aur store karlo variable me for later use
        rb = GetComponent<Rigidbody2D>();

        // top down game me gravity nahi chahiye, toh gravity scale ko zero kar diya
        rb.gravityScale = 0f;

        // colliders ke saath physics interactions me rotation nahi chahiye, toh freezeRotation true.
        rb.freezeRotation = true;
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
    }

    private void FixedUpdate()
    {
        // fixed update ko physics update ke liye use karte hai, and ye har tick me chalta hai, frame rate se independent.
        // ya pe hum rigidbody ko directly move karenge, toh smooth movement milega.

        rb.velocity = moveInput * moveSpeed;
    }
}