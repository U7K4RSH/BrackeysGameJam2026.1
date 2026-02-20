using UnityEngine;
using System.Collections;

public class CameraShake2D : MonoBehaviour
{
    public static CameraShake2D Instance { get; private set; }

    private Vector3 originalLocalPos;
    private Coroutine running;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        originalLocalPos = transform.localPosition;
    }

    public void Shake(float duration, float strength)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(ShakeRoutine(duration, strength));
    }

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        float t = 0f;
        originalLocalPos = transform.localPosition;

        while (t < duration)
        {
            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;

            transform.localPosition = originalLocalPos + new Vector3(x, y, 0f);

            t += Time.unscaledDeltaTime; // works even if Time.timeScale = 0
            yield return null;
        }

        transform.localPosition = originalLocalPos;
        running = null;
    }
}