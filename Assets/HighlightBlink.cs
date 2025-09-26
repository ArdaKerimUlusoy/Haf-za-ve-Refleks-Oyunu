using UnityEngine;

public class HighlightBlink : MonoBehaviour
{
    public float pulseAmount = 0.08f;
    public float speed = 6f;
    private Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        transform.localScale = baseScale * (1f + Mathf.Sin(Time.time * speed) * pulseAmount);
    }
}
