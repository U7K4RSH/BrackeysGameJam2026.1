using UnityEngine;

public class CameraFollow2D: MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.12f;

    private Vector3 velocity;
    // ye camera ka "smooth movement memory" hai — SmoothDamp isko use karke dheere-dheere follow karata hai

    private void LateUpdate()
    {
        //  player pehle move ho jaaye, phir camera follow kare (warna jitter/lag feel hota)

        if (target == null) return;
        // agar target set hi nahi hai toh kya follow karega? isliye seedha nikal ja

        // Z ko same rakho bhai (-10 usually), warna camera aage-peeche chala gaya toh scene gayab ho jaayega
        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);

        // SmoothDamp = "ekdum teleport nahi",makkhan jaisa smooth slide
        // current position se desired position tak smoothly le jaa yaara.
        transform.position = Vector3.SmoothDamp(
            transform.position,   // abhi camera kaha hai
            desired,              // camera ko kaha hona chahiye (player ke upar)
            ref velocity,         // ye velocity internally update hoti rahegi (smoothness maintain)
            smoothTime            // kitna smooth / kitna slow follow kare (less = fast, more = floaty)
        );
    }

    // Optional: code se target set karna ho toh ye use karo, laundo.
    public void SetTarget(Transform t) => target = t;

}