using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(EnemyManager))]
public class EnemyManagerEditor : Editor
{    private SerializedProperty enemyPrefabsProperty;
    private SerializedProperty spawnPointsProperty;
    private SerializedProperty enemiesPerWaveProperty;
    private SerializedProperty respawnDelayProperty;
    private SerializedProperty maxWavesProperty;
    private SerializedProperty increaseEnemiesPerWaveProperty;
    private SerializedProperty enemyIncreasePerWaveProperty;    private void OnEnable()
    {
        enemyPrefabsProperty = serializedObject.FindProperty("enemyPrefabs");
        spawnPointsProperty = serializedObject.FindProperty("spawnPoints");
        enemiesPerWaveProperty = serializedObject.FindProperty("enemiesPerWave");
        respawnDelayProperty = serializedObject.FindProperty("respawnDelay");
        maxWavesProperty = serializedObject.FindProperty("maxWaves");
        increaseEnemiesPerWaveProperty = serializedObject.FindProperty("increaseEnemiesPerWave");
        enemyIncreasePerWaveProperty = serializedObject.FindProperty("enemyIncreasePerWave");
    }    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EnemyManager manager = (EnemyManager)target;
        bool hasMissingEnemyPrefabs = enemyPrefabsProperty.arraySize == 0;
        bool hasMissingSpawnPoints = spawnPointsProperty.arraySize == 0;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Настройки менеджера врагов", EditorStyles.boldLabel);
        
        if (hasMissingEnemyPrefabs || hasMissingSpawnPoints)
        {
            EditorGUILayout.HelpBox("Внимание! Не все обязательные поля заполнены:", MessageType.Warning);
            
            if (hasMissingEnemyPrefabs)
            {
                EditorGUILayout.HelpBox("Не заданы префабы врагов. Добавьте хотя бы один префаб врага!", MessageType.Error);
            }
            
            if (hasMissingSpawnPoints)
            {
                EditorGUILayout.HelpBox("Не заданы точки спавна. Используйте кнопки ниже для добавления точек спавна!", MessageType.Error);
            }
            
            EditorGUILayout.Space();
        }
        
        EditorGUILayout.LabelField("Префабы врагов", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(enemyPrefabsProperty, true);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Точки спавна", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(spawnPointsProperty, true);
        
        if (GUILayout.Button("Найти все точки спавна в сцене"))
        {
            FindAllSpawnPoints();
        }

        if (GUILayout.Button("Создать 4 точки спавна по углам"))
        {
            CreateFourSpawnPoints();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Настройки волн", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(enemiesPerWaveProperty);
        EditorGUILayout.PropertyField(respawnDelayProperty);
        EditorGUILayout.PropertyField(maxWavesProperty);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Настройки сложности", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(increaseEnemiesPerWaveProperty);
        
        if (increaseEnemiesPerWaveProperty.boolValue)
        {
            EditorGUILayout.PropertyField(enemyIncreasePerWaveProperty);
        }

        if (GUILayout.Button("Найти все точки спавна в сцене"))
        {
            FindAllSpawnPoints();
        }

        if (GUILayout.Button("Создать 4 точки спавна по углам"))
        {
            CreateFourSpawnPoints();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void FindAllSpawnPoints()
    {
        EnemyManager manager = (EnemyManager)target;
        SpawnPoint[] allSpawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        
        if (allSpawnPoints.Length > 0)
        {
            Undo.RecordObject(manager, "Find Spawn Points");
            
            SerializedProperty spawnPoints = serializedObject.FindProperty("spawnPoints");
            spawnPoints.arraySize = allSpawnPoints.Length;
            
            for (int i = 0; i < allSpawnPoints.Length; i++)
            {
                spawnPoints.GetArrayElementAtIndex(i).objectReferenceValue = allSpawnPoints[i].transform;
            }
            
            serializedObject.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(manager);
            Debug.Log($"Найдено {allSpawnPoints.Length} точек спавна в сцене");
        }
        else
        {
            Debug.Log("Точек спавна в сцене не найдено. Создайте их сначала.");
        }
    }

    private void CreateFourSpawnPoints()
    {
        EnemyManager manager = (EnemyManager)target;
        Vector3[] positions = new Vector3[4]
        {
            new Vector3(10, 10, 0),
            new Vector3(-10, 10, 0),
            new Vector3(-10, -10, 0),
            new Vector3(10, -10, 0)
        };

        GameObject spawnPointPrefab = null;
        
        string[] guids = AssetDatabase.FindAssets("t:Prefab SpawnPoint");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            spawnPointPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        Undo.RecordObject(manager, "Create Spawn Points");
        
        SerializedProperty spawnPoints = serializedObject.FindProperty("spawnPoints");
        spawnPoints.arraySize = 4;
        
        for (int i = 0; i < 4; i++)
        {
            GameObject spawnPoint;
            
            if (spawnPointPrefab != null)
            {
                spawnPoint = PrefabUtility.InstantiatePrefab(spawnPointPrefab) as GameObject;
            }
            else
            {
                spawnPoint = new GameObject($"SpawnPoint_{i+1}");
                spawnPoint.AddComponent<SpawnPoint>();
            }
            
            Undo.RegisterCreatedObjectUndo(spawnPoint, "Create Spawn Point");
            
            spawnPoint.transform.position = positions[i];
            spawnPoints.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoint.transform;
        }
        
        serializedObject.ApplyModifiedProperties();
        
        EditorUtility.SetDirty(manager);
        Debug.Log("Созданы 4 точки спавна по углам сцены");
    }
}
#endif
