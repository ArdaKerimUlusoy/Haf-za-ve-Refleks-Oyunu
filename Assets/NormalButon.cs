using System.Collections;
using UnityEngine;

public class NormalButton : MonoBehaviour
{
    private ButtonManager manager;
    private int buttonID;
    private Vector3 originalScale;
    private Vector3 startPos;
    private bool initialized = false;

    [Header("Press feedback")]
    public float pressScale = 0.9f;
    public float pressTime = 0.12f;

    [Header("Float animation")]
    public float floatAmplitude = 0.05f;
    public float floatSpeed = 2f;

    void Awake()
    {
        originalScale = transform.localScale;
        startPos = transform.position;
    }

    public void Init(ButtonManager mgr, int id)
    {
        manager = mgr;
        buttonID = id;
        initialized = true;
    }

    void Update()
    {
        // Yukarý–aþaðý hafif sallanma
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
    }

    void OnMouseDown()
    {
        if (!initialized) return;
        manager.ButtonClicked(buttonID);
    }

    public void PressEffect()
    {
        StopAllCoroutines();
        StartCoroutine(PressAnim());
    }

    IEnumerator PressAnim()
    {
        float t = 0f;
        Vector3 start = originalScale;
        Vector3 target = originalScale * pressScale;

        while (t < pressTime)
        {
            transform.localScale = Vector3.Lerp(start, target, t / pressTime);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localScale = target;

        t = 0f;
        while (t < pressTime)
        {
            transform.localScale = Vector3.Lerp(target, start, t / pressTime);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localScale = start;
    }
}
