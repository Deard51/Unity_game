using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void NewGame()
    {
        PlayerPrefs.DeleteAll();       
        SceneManager.LoadScene("SampleScene");
    }

    public void LoadGame()
    {    
        if (PlayerPrefs.HasKey("SavedScene"))
        {SceneManager.LoadScene("SampleScene"); 
        }
        else
        {
            Debug.Log("Нет сохранённой игры!");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
