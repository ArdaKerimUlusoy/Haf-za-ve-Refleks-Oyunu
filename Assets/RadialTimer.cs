using UnityEngine;
using UnityEngine.UI;
using System;
#if TMP_PRESENT
using TMPro;
#endif

[RequireComponent(typeof(Image))]
public class RadialTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Süre (saniye)")]
    public float duration = 3f;
    [Tooltip("Otomatik baþla mý?")]
    public bool autoStart = true;
    [Tooltip("Tersten (geriye sayým) mi? Eðer true, fillAmount 1 -> 0' a düþer.")]
    public bool reverse = false;

    [Header("References (optional)")]
    public Image radialImage; // inspector'a sürükle veya otomatik atanýr
    public Text timeText;     // opsiyonel: UnityEngine.UI.Text
#if TMP_PRESENT
    public TextMeshProUGUI tmpTimeText; // opsiyonel TMP
#endif

    [Header("Behavior")]
    public bool useRealtime = false; // pause etkilenmesin istiyorsan true

    public event Action OnTimerComplete;

    float elapsed = 0f;
    bool running = false;

    void Reset()
    {
        radialImage = GetComponent<Image>();
    }

    void Awake()
    {
        if (radialImage == null)
            radialImage = GetComponent<Image>();

        // ensure image type is Filled and radial configured
        if (radialImage != null && radialImage.type != Image.Type.Filled)
            radialImage.type = Image.Type.Filled;
    }

    void Start()
    {
        if (autoStart)
            StartTimer();
        else
            ApplyFill();
    }

    void Update()
    {
        if (!running) return;

        float delta = useRealtime ? Time.unscaledDeltaTime : Time.deltaTime;
        elapsed += delta;

        if (elapsed >= duration)
        {
            elapsed = duration;
            running = false;
            ApplyFill();
            OnTimerComplete?.Invoke();
            return;
        }

        ApplyFill();
    }

    void ApplyFill()
    {
        if (radialImage == null) return;

        float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
        float fill = reverse ? 1f - t : t;
        radialImage.fillAmount = fill;

        // update text if any
        float remaining = Mathf.Max(0f, duration - elapsed);
        if (timeText != null)
            timeText.text = Mathf.CeilToInt(remaining).ToString();
#if TMP_PRESENT
        if (tmpTimeText != null)
            tmpTimeText.text = Mathf.CeilToInt(remaining).ToString();
#endif
    }

    // Public controls
    public void StartTimer(float customDuration = -1f)
    {
        if (customDuration > 0f) duration = customDuration;
        elapsed = 0f;
        running = true;
        ApplyFill();
    }

    public void StopTimer()
    {
        running = false;
    }

    public void ResetTimer()
    {
        elapsed = 0f;
        running = false;
        ApplyFill();
    }

    public void PauseTimer()
    {
        running = false;
    }

    public void ResumeTimer()
    {
        if (elapsed < duration)
            running = true;
    }

    public bool IsRunning => running;
    public float Elapsed => elapsed;
    public float Duration => duration;
}
