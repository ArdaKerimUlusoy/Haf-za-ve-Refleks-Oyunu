using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if TMP_PRESENT
using TMPro;
#endif

public class ButtonManager : MonoBehaviour
{
    [Header("Spawn & Prefabs")]
    public Transform[] spawnPoints;
    public GameObject buttonPrefab;
    public GameObject highlightPrefab;

    [Header("Level & Difficulty")]
    public int maxLevel = 100;
    public float startDelay = 0.8f;
    public float startHighlightDuration = 0.8f;
    public float endHighlightDuration = 0.25f;
    public float startBetweenDelay = 0.4f;
    public float endBetweenDelay = 0.1f;

    [Header("Manual Start (Tanýtým Ýçin)")]
    public bool useManualLevel = false;
    public int manualLevel = 1;

    [Header("UI Elements")]
    public RadialTimer radialTimer;
    public Text levelText;
#if TMP_PRESENT
    public TextMeshProUGUI levelTMP;
#endif
    public GameObject tryAgainPanel;
    public Button tryAgainButton;
    public Button mainMenuButton;

    private List<GameObject> currentButtons = new List<GameObject>();
    private List<int> targetSequence = new List<int>();
    private List<int> playerSequence = new List<int>();
    private int currentLevel = 1;
    private bool inputEnabled = false;
    private Transform buttonsParent;

    void Start()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            Debug.LogError("ButtonManager: spawnPoints not set!");

        buttonsParent = new GameObject("ButtonsContainer").transform;
        buttonsParent.SetParent(transform);

        // Eðer manuel baþlatma aktifse PlayerPrefs yerine manuelLevel kullan
        if (useManualLevel)
            currentLevel = Mathf.Clamp(manualLevel, 1, maxLevel);
        else
            currentLevel = PlayerPrefs.GetInt("LoadLevel", 1);

        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(false);
        if (tryAgainButton != null) tryAgainButton.gameObject.SetActive(false);
        if (tryAgainPanel != null) tryAgainPanel.SetActive(false);

        StartLevel();

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        if (tryAgainButton != null)
            tryAgainButton.onClick.AddListener(RestartLevel);
    }

    void StartLevel()
    {
        StopAllCoroutines();
        inputEnabled = false;
        ClearButtons();
        SpawnButtonsForLevel(currentLevel);
        CreateTargetSequence(currentLevel);
        UpdateLevelText();

        float totalTime = GetTimeForLevel(currentLevel);

        if (radialTimer != null)
        {
            radialTimer.OnTimerComplete -= HandleTimeUp;
            radialTimer.OnTimerComplete += HandleTimeUp;
            radialTimer.StartTimer(totalTime);
        }

        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(false);
        if (tryAgainButton != null) tryAgainButton.gameObject.SetActive(false);
        if (tryAgainPanel != null) tryAgainPanel.SetActive(false);

        StartCoroutine(ShowSequence());
    }

    void UpdateLevelText()
    {
        if (levelText != null)
            levelText.text = "Level: " + currentLevel;
#if TMP_PRESENT
        if (levelTMP != null)
            levelTMP.text = "Level: " + currentLevel;
#endif
    }

    void HandleTimeUp()
    {
        inputEnabled = false;

        PlayerPrefs.SetInt("SavedLevel", currentLevel);
        PlayerPrefs.Save();

        if (tryAgainPanel != null)
            tryAgainPanel.SetActive(true);

        if (tryAgainButton != null)
            tryAgainButton.gameObject.SetActive(true);

        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(true);
    }

    public void RestartLevel()
    {
        if (tryAgainPanel != null)
            tryAgainPanel.SetActive(false);

        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(false);

        if (tryAgainButton != null)
            tryAgainButton.gameObject.SetActive(false);

        StartLevel();
    }

    public void ReturnToMainMenu()
    {
        PlayerPrefs.SetInt("SavedLevel", currentLevel);
        PlayerPrefs.Save();
        SceneManager.LoadScene("mainmenu");
    }

    void SpawnButtonsForLevel(int level)
    {
        int buttonCount = GetButtonCountForLevel(level);
        buttonCount = Mathf.Clamp(buttonCount, 1, spawnPoints.Length);

        List<int> available = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
            available.Add(i);

        for (int i = 0; i < buttonCount; i++)
        {
            if (available.Count == 0) break;
            int pickAt = Random.Range(0, available.Count);
            int spawnIndex = available[pickAt];
            available.RemoveAt(pickAt);

            Transform sp = spawnPoints[spawnIndex];
            GameObject go = Instantiate(buttonPrefab, sp.position, sp.rotation, buttonsParent);
            go.name = "Button_" + spawnIndex;

            NormalButton nb = go.GetComponent<NormalButton>();
            if (nb != null) nb.Init(this, i);
            currentButtons.Add(go);
        }
    }

    void CreateTargetSequence(int level)
    {
        targetSequence.Clear();
        int seqLen = GetSequenceLengthForLevel(level);
        for (int i = 0; i < seqLen; i++)
        {
            int idx = Random.Range(0, currentButtons.Count);
            targetSequence.Add(idx);
        }
    }

    IEnumerator ShowSequence()
    {
        inputEnabled = false;
        yield return new WaitForSeconds(startDelay);

        float t = (currentLevel - 1f) / Mathf.Max(1, maxLevel - 1f);
        float highlightDuration = Mathf.Lerp(startHighlightDuration, endHighlightDuration, t);
        float between = Mathf.Lerp(startBetweenDelay, endBetweenDelay, t);

        for (int i = 0; i < targetSequence.Count; i++)
        {
            int btnIdx = targetSequence[i];
            if (btnIdx < 0 || btnIdx >= currentButtons.Count) continue;

            GameObject buttonObj = currentButtons[btnIdx];
            buttonObj.SetActive(false);

            Vector3 pos = buttonObj.transform.position + Vector3.up * 0.05f;
            GameObject hl = Instantiate(highlightPrefab, pos, Quaternion.identity, buttonsParent);
            hl.name = "HL_" + i;

            yield return new WaitForSeconds(highlightDuration);

            Destroy(hl);
            buttonObj.SetActive(true);

            yield return new WaitForSeconds(between);
        }

        playerSequence.Clear();
        inputEnabled = true;
    }

    public void ButtonClicked(int buttonID)
    {
        if (!inputEnabled) return;
        if (buttonID < 0 || buttonID >= currentButtons.Count) return;

        playerSequence.Add(buttonID);
        currentButtons[buttonID].GetComponent<NormalButton>()?.PressEffect();

        if (playerSequence.Count >= targetSequence.Count)
        {
            inputEnabled = false;
            if (IsSequenceCorrect())
            {
                currentLevel = Mathf.Min(currentLevel + 1, maxLevel);
                StartCoroutine(LevelWinRoutine());
            }
            else
            {
                StartCoroutine(HandleFailure());
            }
        }
    }

    IEnumerator LevelWinRoutine()
    {
        yield return new WaitForSeconds(0.6f);

        PlayerPrefs.SetInt("SavedLevel", currentLevel);
        PlayerPrefs.Save();

        StartLevel();
    }

    IEnumerator HandleFailure()
    {
        yield return new WaitForSeconds(0.5f);
        playerSequence.Clear();
        CreateTargetSequence(currentLevel);
        StartCoroutine(ShowSequence());
    }

    bool IsSequenceCorrect()
    {
        if (playerSequence.Count != targetSequence.Count) return false;
        for (int i = 0; i < targetSequence.Count; i++)
        {
            if (playerSequence[i] != targetSequence[i]) return false;
        }
        return true;
    }

    int GetButtonCountForLevel(int level)
    {
        if (level <= 5) return 5;
        if (level <= 20) return 10;
        if (level <= 60) return 15;
        return 25;
    }

    int GetSequenceLengthForLevel(int level)
    {
        if (level <= 5) return 3;
        if (level <= 20) return 5;
        if (level <= 40) return 6;
        if (level <= 60) return 8;
        if (level <= 80) return 10;
        return 12;
    }

    float GetTimeForLevel(int level)
    {
        if (level <= 5) return 7f;
        if (level <= 20) return 20f;
        if (level <= 60) return 350f;
        return 50f;
    }

    void ClearButtons()
    {
        if (buttonsParent != null)
        {
            foreach (Transform child in buttonsParent)
            {
                Destroy(child.gameObject);
            }
        }
        currentButtons.Clear();
    }
}
