using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // En son kald���n leveli al, yoksa 1. levelden ba�la
        int savedLevel = PlayerPrefs.GetInt("SavedLevel", 1);
        PlayerPrefs.SetInt("LoadLevel", savedLevel);

        // Oyun sahnesini y�kle
        SceneManager.LoadScene("SampleScene"); // Buraya oyun sahnenin ad�n� yaz
    }

    public void QuitGame()
    {
        Debug.Log("Oyun kapat�l�yor...");
        Application.Quit();
    }
}
