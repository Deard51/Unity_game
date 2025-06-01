using UnityEngine;

public class LoadSavedGame : MonoBehaviour
{
    private void Start()
    {
        if (PlayerPrefs.HasKey("SavedScene"))
        {
            // Загружаем позицию игрока
            if (PlayerPrefs.HasKey("PlayerX"))
            {
                float x = PlayerPrefs.GetFloat("PlayerX");
                float y = PlayerPrefs.GetFloat("PlayerY");
                float z = PlayerPrefs.GetFloat("PlayerZ");
                transform.position = new Vector3(x, y, z);
            }

            // Загружаем параметры игрока из класса Player
            Player player = GetComponent<Player>();
            if (player != null)
            {
                if (PlayerPrefs.HasKey("PlayerHealth"))
                {
                    player.LoadHealth(PlayerPrefs.GetInt("PlayerHealth"));
                }
                
                // Можно добавить загрузку других параметров, если нужно
                // Например: player.LoadMaxHealth(PlayerPrefs.GetInt("PlayerMaxHealth"));
            }
        }
    }
}