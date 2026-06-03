using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    private string saveFilePath;

    void Awake()
    {
        // Aceeași locație pe care o vom folosi și în joc pentru salvare
        saveFilePath = Application.persistentDataPath + "/savefile.json";
    }

    public void NewGame()
    {
        // Ștergem salvarea veche ca să o luăm de la 0
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Salvare ștearsă! Începem de la zero.");
        }

        // Încărcăm scena jocului. (Schimbă "SampleScene" dacă scena ta cu jocul se numește altfel!)
        SceneManager.LoadScene("SampleScene");
    }

    public void LoadGame()
    {
        // Permitem intrarea în joc doar dacă există o salvare
        if (File.Exists(saveFilePath))
        {
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            Debug.Log("Nu există nicio salvare!");
        }
    }

    public void QuitGame()
    {
        Debug.Log("Ieșim din joc...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}