using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    public Player player;
    public Button restartButton;
    public Button menuButton;
    public Button quitButton;

    private string currentSceneName;

    private void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        
        // Назначаем обработчики кнопок
        restartButton.onClick.AddListener(RestartLevel);
        menuButton.onClick.AddListener(SaveAndQuitToMenu);
        quitButton.onClick.AddListener(QuitGame);

        // Скрываем меню при старте
        pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        
        if (player != null)
        {
            player.enabled = true;
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        
        if (player != null)
        {
            player.enabled = false;
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(currentSceneName);
    }

    public void SaveAndQuitToMenu()
    {
        SaveGame();
        Time.timeScale = 1f;
        
        if (SceneUtility.GetBuildIndexByScenePath("MainMenu") >= 0)
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            Debug.LogError("Сцена MainMenu не найдена в Build Settings!");
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }

    public void QuitGame()
    {
        SaveGame();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void SaveGame()
    {
        try
        {
            PlayerPrefs.SetString("SavedScene", currentSceneName);
            
            if (player != null)
            {
                PlayerPrefs.SetFloat("PlayerX", player.transform.position.x);
                PlayerPrefs.SetFloat("PlayerY", player.transform.position.y);
                PlayerPrefs.SetFloat("PlayerZ", player.transform.position.z);
                PlayerPrefs.SetInt("PlayerHealth", player.GetCurrentHealth());
            }
            
            PlayerPrefs.Save();
            Debug.Log("Игра сохранена!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка сохранения: {e.Message}");
        }
    }
}