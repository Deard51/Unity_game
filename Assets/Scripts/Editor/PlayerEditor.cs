using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

[CustomEditor(typeof(Player))]
public class PlayerEditor : Editor
{
    SerializedProperty heartImages;
    SerializedProperty fullHeartSprite;
    SerializedProperty emptyHeartSprite;
    SerializedProperty healthUISortingLayerName;
    SerializedProperty healthUISortingOrder;
    
    // Остальные свойства
    SerializedProperty movingSpeed;
    SerializedProperty minMovingSpeed;
    SerializedProperty dashSpeed;
    SerializedProperty dashDuration;
    SerializedProperty dashCooldown;
    SerializedProperty dashEffectPrefab;
    SerializedProperty attackDamage;
    SerializedProperty attackRadius;
    SerializedProperty enemyLayer;
    SerializedProperty attackPoint;
    SerializedProperty attackCooldown;
    SerializedProperty attackEffectPrefab;
    SerializedProperty maxHealth;
    SerializedProperty bowPrefab;
    SerializedProperty arrowPrefab;
    SerializedProperty bowOffset;
    SerializedProperty bowRotationSpeed;
    SerializedProperty bowParent;
    SerializedProperty playerLayer;
    SerializedProperty enemyPhysicsLayer;

    private void OnEnable()
    {
        // Получаем ссылки на свойства из сериализованного класса
        heartImages = serializedObject.FindProperty("heartImages");
        fullHeartSprite = serializedObject.FindProperty("fullHeartSprite");
        emptyHeartSprite = serializedObject.FindProperty("emptyHeartSprite");
        healthUISortingLayerName = serializedObject.FindProperty("healthUISortingLayerName");
        healthUISortingOrder = serializedObject.FindProperty("healthUISortingOrder");
        
        movingSpeed = serializedObject.FindProperty("movingSpeed");
        minMovingSpeed = serializedObject.FindProperty("minMovingSpeed");
        dashSpeed = serializedObject.FindProperty("dashSpeed");
        dashDuration = serializedObject.FindProperty("dashDuration");
        dashCooldown = serializedObject.FindProperty("dashCooldown");
        dashEffectPrefab = serializedObject.FindProperty("dashEffectPrefab");
        attackDamage = serializedObject.FindProperty("attackDamage");
        attackRadius = serializedObject.FindProperty("attackRadius");
        enemyLayer = serializedObject.FindProperty("enemyLayer");
        attackPoint = serializedObject.FindProperty("attackPoint");
        attackCooldown = serializedObject.FindProperty("attackCooldown");
        attackEffectPrefab = serializedObject.FindProperty("attackEffectPrefab");
        maxHealth = serializedObject.FindProperty("maxHealth");
        bowPrefab = serializedObject.FindProperty("bowPrefab");
        arrowPrefab = serializedObject.FindProperty("arrowPrefab");
        bowOffset = serializedObject.FindProperty("bowOffset");
        bowRotationSpeed = serializedObject.FindProperty("bowRotationSpeed");
        bowParent = serializedObject.FindProperty("bowParent");
        playerLayer = serializedObject.FindProperty("playerLayer");
        enemyPhysicsLayer = serializedObject.FindProperty("enemyPhysicsLayer");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(movingSpeed);
        EditorGUILayout.PropertyField(minMovingSpeed);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dash", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(dashSpeed);
        EditorGUILayout.PropertyField(dashDuration);
        EditorGUILayout.PropertyField(dashCooldown);
        EditorGUILayout.PropertyField(dashEffectPrefab);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Attack", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(attackDamage);
        EditorGUILayout.PropertyField(attackRadius);
        EditorGUILayout.PropertyField(enemyLayer);
        EditorGUILayout.PropertyField(attackPoint);
        EditorGUILayout.PropertyField(attackCooldown);
        EditorGUILayout.PropertyField(attackEffectPrefab);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Health", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(maxHealth);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Health UI", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(heartImages, true);        EditorGUILayout.PropertyField(fullHeartSprite);
        EditorGUILayout.PropertyField(emptyHeartSprite);
        
        // Создаем массивы имен и индексов слоев
        string[] sortingLayerNames = new string[SortingLayer.layers.Length];
        for (int i = 0; i < SortingLayer.layers.Length; i++)
        {
            sortingLayerNames[i] = SortingLayer.layers[i].name;
        }
        
        // Находим текущий индекс слоя
        int currentIndex = -1;
        string currentLayerName = healthUISortingLayerName.stringValue;
        for (int i = 0; i < sortingLayerNames.Length; i++)
        {
            if (sortingLayerNames[i] == currentLayerName)
            {
                currentIndex = i;
                break;
            }
        }
        
        // Если слой не найден, используем Default
        if (currentIndex == -1)
        {
            currentIndex = 0; // Default layer
        }
        
        // Создаем выпадающий список
        EditorGUI.BeginChangeCheck();
        int newLayerIndex = EditorGUILayout.Popup("Sorting Layer", currentIndex, sortingLayerNames);
        if (EditorGUI.EndChangeCheck())
        {
            // Если пользователь выбрал новый слой, обновляем свойство
            healthUISortingLayerName.stringValue = sortingLayerNames[newLayerIndex];
            serializedObject.ApplyModifiedProperties();
        }
        
        // Отображаем поле для ввода порядка сортировки
        EditorGUILayout.PropertyField(healthUISortingOrder);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Bow", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(bowPrefab);
        EditorGUILayout.PropertyField(arrowPrefab);
        EditorGUILayout.PropertyField(bowOffset);
        EditorGUILayout.PropertyField(bowRotationSpeed);
        EditorGUILayout.PropertyField(bowParent);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Physics Layers", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(playerLayer);
        EditorGUILayout.PropertyField(enemyPhysicsLayer);
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private string[] GetSortingLayerNames()
    {
        // Получаем все имена слоев сортировки из TagManager
        List<string> layerNames = new List<string>();
        foreach (SortingLayer layer in SortingLayer.layers)
        {
            layerNames.Add(layer.name);
        }
        return layerNames.ToArray();
    }
    
    private int[] GetSortingLayerIDs()
    {
        // Получаем все ID слоев сортировки
        List<int> layerIDs = new List<int>();
        foreach (SortingLayer layer in SortingLayer.layers)
        {
            layerIDs.Add(layer.id);
        }
        return layerIDs.ToArray();
    }
    
    private int GetSortingLayerIndex(string layerName, string[] layerNames)
    {
        // Ищем индекс текущего выбранного слоя
        for (int i = 0; i < layerNames.Length; i++)
        {
            if (layerNames[i] == layerName)
                return i;
        }
        return 0; // Возвращаем 0, если не нашли (default layer)
    }
}
