using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicSource;
    public Slider volumeSlider;
    public GameObject sliderPanel;   // Slider’ýn bulunduðu panel
    public Button musicButton;       // Müzik simgesi butonu

    private static MusicManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.Play();
        }

        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        musicSource.volume = savedVolume;
        if (volumeSlider != null)
            volumeSlider.value = savedVolume;

        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(SetVolume);

        if (musicButton != null)
            musicButton.onClick.AddListener(ToggleSlider);

        if (sliderPanel != null)
            sliderPanel.SetActive(false); // Baþlangýçta kapalý
    }

    public void SetVolume(float value)
    {
        if (musicSource != null)
            musicSource.volume = value;

        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void ToggleSlider()
    {
        if (sliderPanel != null)
            sliderPanel.SetActive(!sliderPanel.activeSelf);
    }
}
