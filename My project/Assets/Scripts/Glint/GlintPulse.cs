using UnityEngine;

public class GlintPulse : MonoBehaviour
{
    [Header("Alpha")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float alphaSpeed = 3f;
    [SerializeField] private float minA = 0.15f;
    [SerializeField] private float maxA = 1f;

    [Header("Scale")]
    [SerializeField] private float scaleSpeed = 3f;
    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.1f;

    private Vector3 baseScale;

    private void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    private void Update()
    {
        // -------- Alpha pulse --------
        if (sr != null)
        {
            float tA = (Mathf.Sin(Time.time * alphaSpeed) + 1f) * 0.5f; // 0..1
            float a = Mathf.Lerp(minA, maxA, tA);

            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }

        // -------- Scale pulse --------
        float tS = (Mathf.Sin(Time.time * scaleSpeed) + 1f) * 0.5f; // 0..1
        float s = Mathf.Lerp(minScale, maxScale, tS);

        transform.localScale = baseScale * s;
    }
}