using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveIndicator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private float showDuration = 3f;
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float fadeOutTime = 0.5f;    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color endColor = new Color(1, 0.5f, 0, 1);
    
    private CanvasGroup canvasGroup;
    private EnemyManager enemyManager;

    private void Awake()
    {        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0;
        enemyManager = FindAnyObjectByType<EnemyManager>();
        if (enemyManager != null)
        {
            enemyManager.OnNewWave += EnemyManager_OnNewWave;
        }
        else
        {
            Debug.LogWarning("EnemyManager не найден в сцене, индикатор волны не будет работать.");
        }
    }
      private void OnDestroy()
    {
        if (enemyManager != null)
        {
            enemyManager.OnNewWave -= EnemyManager_OnNewWave;
        }
    }
    private void EnemyManager_OnNewWave(object sender, EnemyManager.WaveEventArgs e)
    {
        // Обновляем текст с номером волны
        waveText.text = $"Волна {e.WaveNumber}";
        
        // Меняем цвет в зависимости от номера волны
        if (e.WaveNumber > 1)
        {
            float t = Mathf.Clamp01((e.WaveNumber - 1) / 10f); // Градуальное изменение до 10 волны
            waveText.color = Color.Lerp(startColor, endColor, t);
        }
        else
        {
            waveText.color = startColor;
        }
        
        // Показываем индикатор
        StopAllCoroutines();
        StartCoroutine(ShowWaveIndicator());
    }
    
    // Корутина для показа и скрытия индикатора
    private IEnumerator ShowWaveIndicator()
    {
        // Плавное появление
        float timeElapsed = 0;
        while (timeElapsed < fadeInTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, timeElapsed / fadeInTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;
        
        // Ждем указанное время
        yield return new WaitForSeconds(showDuration);
        
        // Плавное исчезновение
        timeElapsed = 0;
        while (timeElapsed < fadeOutTime)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, timeElapsed / fadeOutTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0;
    }
}
