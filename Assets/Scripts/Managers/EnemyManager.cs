using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public class WaveEventArgs : EventArgs
    {
        public int WaveNumber { get; private set; }
        public int EnemyCount { get; private set; }

        public WaveEventArgs(int waveNumber, int enemyCount)
        {
            WaveNumber = waveNumber;
            EnemyCount = enemyCount;
        }    }

    public event EventHandler<WaveEventArgs> OnNewWave;

    [Header("Настройки спавна")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int enemiesPerWave = 3;
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private int maxWaves = 10;
    [SerializeField] private bool increaseEnemiesPerWave = true;
    [SerializeField] private int enemyIncreasePerWave = 1;

    private List<EnemyEntity> activeEnemies = new List<EnemyEntity>();
    private int currentWave = 0;
    private bool isRespawning = false;    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }    private void Start()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("EnemyManager не настроен! Нужно назначить префабы врагов (enemyPrefabs) в инспекторе.");
            return;
        }
        
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("EnemyManager не настроен! Нужно назначить точки спавна (spawnPoints) в инспекторе.");
            Debug.Log("Подсказка: Используйте кнопки 'Найти все точки спавна в сцене' или 'Создать 4 точки спавна по углам' в инспекторе.");
            return;
        }

        EnemyEntity[] existingEnemies = FindObjectsByType<EnemyEntity>(FindObjectsSortMode.None);
        
        if (existingEnemies.Length > 0)
        {
            Debug.Log($"Найдено {existingEnemies.Length} существующих врагов в сцене");
            
            foreach (var enemy in existingEnemies)
            {
                RegisterEnemy(enemy);
            }
        }
        else
        {
            Debug.Log("Врагов не найдено, спавним начальную волну");
            SpawnEnemyWave();
        }
    }    public void RegisterEnemy(EnemyEntity enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
            enemy.OnEnemyDeath += Enemy_OnDeath;
        }
    }

    private void Enemy_OnDeath(object sender, System.EventArgs e)
    {
        if (sender is EnemyEntity enemy)
        {
            enemy.OnEnemyDeath -= Enemy_OnDeath;
            
            activeEnemies.Remove(enemy);
            
            CheckAllEnemiesDead();
        }
    }    private void CheckAllEnemiesDead()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
        
        if (activeEnemies.Count == 0 && !isRespawning)
        {
            Debug.Log("Все враги уничтожены! Подготовка новой волны...");
            
            StartCoroutine(RespawnTimer());
        }
    }

    private IEnumerator RespawnTimer()
    {
        isRespawning = true;
        
        Debug.Log($"Следующая волна врагов через {respawnDelay} секунд...");
        yield return new WaitForSeconds(respawnDelay);
        
        // Спавним новую волну врагов
        SpawnEnemyWave();
        
        isRespawning = false;
    }    private void SpawnEnemyWave()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Невозможно создать волну врагов: не настроены префабы врагов или точки спавна!");
            return;
        }
        
        currentWave++;
        Debug.Log($"Спавн волны {currentWave}");
          if (maxWaves > 0 && currentWave > maxWaves)
        {
            Debug.Log("Достигнуто максимальное количество волн!");
            return;
        }        int enemiesToSpawn = enemiesPerWave;
        
        if (increaseEnemiesPerWave && currentWave > 1)
        {
            enemiesToSpawn += enemyIncreasePerWave * (currentWave - 1);
            Debug.Log($"Увеличение сложности: в волне {currentWave} будет {enemiesToSpawn} врагов");
        }
        
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
        }
        OnNewWave?.Invoke(this, new WaveEventArgs(currentWave, enemiesToSpawn));
    }    private void SpawnEnemy()
    {
        if (enemyPrefabs == null)
        {
            Debug.LogError("Массив префабов врагов (enemyPrefabs) не назначен! Пожалуйста, назначьте префабы в инспекторе для EnemyManager.");
            return;
        }
        
        if (spawnPoints == null)
        {
            Debug.LogError("Массив точек спавна (spawnPoints) не назначен! Пожалуйста, назначьте точки спавна в инспекторе для EnemyManager.");
            return;
        }
        
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogError("Не заданы префабы врагов или точки спавна! Массивы пусты.");
            return;
        }
          GameObject randomPrefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
        
        Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        
        GameObject enemyObject = Instantiate(randomPrefab, spawnPoint.position, Quaternion.identity);
        EnemyEntity enemyEntity = enemyObject.GetComponent<EnemyEntity>();
        
        if (enemyEntity != null)
        {            RegisterEnemy(enemyEntity);
        }
        else
        {
            Debug.LogError("У созданного врага отсутствует компонент EnemyEntity!");
        }
    }    private void OnDestroy()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                enemy.OnEnemyDeath -= Enemy_OnDeath;
            }
        }
    }
}
