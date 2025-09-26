using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // En son kaldýðýn leveli al, yoksa 1. levelden baþla
        int savedLevel = PlayerPrefs.GetInt("SavedLevel", 1);
        PlayerPrefs.SetInt("LoadLevel", savedLevel);

        // Oyun sahnesini yükle
        SceneManager.LoadScene("SampleScene"); // Buraya oyun sahnenin adýný yaz
    }

    public void QuitGame()
    {
        Debug.Log("Oyun kapatýlýyor...");
        Application.Quit();
    }
}
