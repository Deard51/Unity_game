using UnityEngine;
using System;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("Movement")]
    [SerializeField] private float movingSpeed = 6f;
    [SerializeField] private float minMovingSpeed = 0.1f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private GameObject dashEffectPrefab;

    [Header("Attack")]
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private GameObject attackEffectPrefab;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;    [Header("Health UI")]    [SerializeField] private List<Image> heartImages;    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;
    [SerializeField] private string healthUISortingLayerName = "UI";
    [SerializeField] private int healthUISortingOrder = 10;

    [Header("Bow")]
    [SerializeField] private GameObject bowPrefab;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float bowOffset = 0.5f;
    [SerializeField] private float bowRotationSpeed = 10f;
    [SerializeField] private Transform bowParent;

    [Header("Physics Layers")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask enemyPhysicsLayer;

    private Vector2 inputVector;
    private Rigidbody2D rb;
    private bool isRunning = false;
    private bool isDashing = false;
    private float dashTimeRemaining;
    private float dashCooldownRemaining;
    private Vector2 dashDirection;
    private float nextAttackTime = 0f;
    private PolygonCollider2D attackCollider;
    private bool isAttacking = false;
    private GameObject bowInstance;
    private bool isBowActive = false;
    private Vector2 bowDirection;
    private Transform arrowSpawnPoint;

    public event EventHandler OnAttack;
    public event EventHandler OnDash;
    public event EventHandler OnTakeDamage;
    public event EventHandler OnDeath;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();

        attackCollider = GetComponent<PolygonCollider2D>();
        if (attackCollider == null)
        {
            Debug.LogWarning("Player is missing PolygonCollider2D for attacks. Please add one in the inspector.");
        }
        else
        {
            attackCollider.enabled = false;
        }

        int playerLayerIndex = (int)Mathf.Log(playerLayer.value, 2);
        int enemyLayerIndex = (int)Mathf.Log(enemyPhysicsLayer.value, 2);

        if (playerLayerIndex < 0 || enemyLayerIndex < 0)
        {
            Debug.LogWarning("[DASH] Layer masks not properly configured. Check Inspector settings for Player and Enemy layers!");
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        SetHeartImagesSortingLayer();

        GameInput.Instance.OnAttackAction += GameInput_OnAttackAction;
        GameInput.Instance.OnDashAction += GameInput_OnDashAction;
        GameInput.Instance.OnBowAttackStarted += GameInput_OnBowAttackStarted;
        GameInput.Instance.OnBowAttackCanceled += GameInput_OnBowAttackCanceled;
    }

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnAttackAction -= GameInput_OnAttackAction;
            GameInput.Instance.OnDashAction -= GameInput_OnDashAction;
            GameInput.Instance.OnBowAttackStarted -= GameInput_OnBowAttackStarted;
            GameInput.Instance.OnBowAttackCanceled -= GameInput_OnBowAttackCanceled;
        }

        if (bowInstance != null)
        {
            Destroy(bowInstance);
        }
    }

    private void OnDisable()
    {
        if (isDashing)
        {
            EnableEnemyCollisionsAfterDash();
            isDashing = false;
        }
    }

    private void Update()
    {
        if (dashCooldownRemaining > 0)
        {
            dashCooldownRemaining -= Time.deltaTime;
        }

        if (isDashing)
        {
            dashTimeRemaining -= Time.deltaTime;
            if (dashTimeRemaining <= 0)
            {
                isDashing = false;
                rb.linearVelocity = Vector2.zero;
                EnableEnemyCollisionsAfterDash();
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F key pressed - testing attack manually");
            PerformAttack();
        }

        if (isBowActive && bowInstance != null)
        {
            UpdateBowDirection();
            RotateBowTowardsMouse();
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            HandleDashing();
        }
        else
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        inputVector = GameInput.Instance.GetMovmentVector();
        rb.linearVelocity = inputVector * movingSpeed;

        if (rb.bodyType != RigidbodyType2D.Dynamic)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        rb.freezeRotation = true;
        isRunning = inputVector.magnitude > minMovingSpeed;
    }

    private void HandleDashing()
    {
        rb.linearVelocity = dashDirection * dashSpeed;
    }

    private void GameInput_OnAttackAction(object sender, System.EventArgs e)
    {
        PerformAttack();
    }

    private void GameInput_OnDashAction(object sender, System.EventArgs e)
    {
        PerformDash();
    }

    private void GameInput_OnBowAttackStarted(object sender, System.EventArgs e)
    {
        ShowBow();
    }

    private void GameInput_OnBowAttackCanceled(object sender, System.EventArgs e)
    {
        ShootArrow();
        HideBow();
    }

    private void PerformAttack()
    {
        if (Time.time < nextAttackTime || isAttacking)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        isAttacking = true;
        OnAttack?.Invoke(this, EventArgs.Empty);
    }

    public void AttackColliderTurnOn()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
        }
    }

    public void AttackColliderTurnOff()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
        isAttacking = false;
    }

    public void DealDamage()
    {
        Vector2 attackPosition = attackPoint != null ?
            (Vector2)attackPoint.position :
            (Vector2)transform.position;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPosition,
            attackRadius,
            enemyLayer
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyEntity enemyEntity = enemy.GetComponent<EnemyEntity>();
            if (enemyEntity != null)
            {
                enemyEntity.TakeDamage(attackDamage);
                if (attackEffectPrefab != null)
                {
                    Instantiate(
                        attackEffectPrefab,
                        enemy.transform.position,
                        Quaternion.identity
                    );
                }
            }
        }
    }

    private void PerformDash()
    {
        if (dashCooldownRemaining <= 0 && !isDashing)
        {
            dashDirection = inputVector.normalized;

            if (dashDirection.magnitude < 0.1f)
            {
                Vector2 mousePos = GameInput.Instance.GetMouseWorldPosition();
                dashDirection = ((Vector2)transform.position - mousePos).normalized * -1;

                if (dashDirection.magnitude < 0.1f)
                {
                    dashDirection = Vector2.right;
                }
            }

            isDashing = true;
            dashTimeRemaining = dashDuration;
            dashCooldownRemaining = dashCooldown;
            DisableEnemyCollisionsForDash();
            OnDash?.Invoke(this, EventArgs.Empty);

            if (dashEffectPrefab != null)
            {
                Instantiate(dashEffectPrefab, transform.position, Quaternion.identity);
            }
        }
    }

    private void DisableEnemyCollisionsForDash()
    {
        int playerLayerIndex = (int)Mathf.Log(playerLayer.value, 2);
        int enemyLayerIndex = (int)Mathf.Log(enemyPhysicsLayer.value, 2);

        if (playerLayerIndex >= 0 && playerLayerIndex < 32 && enemyLayerIndex >= 0 && enemyLayerIndex < 32)
        {
            Physics2D.IgnoreLayerCollision(playerLayerIndex, enemyLayerIndex, true);
        }
        else
        {
            try
            {
                int defaultPlayerLayer = LayerMask.NameToLayer("Player");
                int defaultEnemyLayer = LayerMask.NameToLayer("Enemy");

                if (defaultPlayerLayer != -1 && defaultEnemyLayer != -1)
                {
                    Physics2D.IgnoreLayerCollision(defaultPlayerLayer, defaultEnemyLayer, true);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DASH] Error disabling collisions: {e.Message}");
            }
        }
    }

    private void EnableEnemyCollisionsAfterDash()
    {
        int playerLayerIndex = (int)Mathf.Log(playerLayer.value, 2);
        int enemyLayerIndex = (int)Mathf.Log(enemyPhysicsLayer.value, 2);

        if (playerLayerIndex >= 0 && playerLayerIndex < 32 && enemyLayerIndex >= 0 && enemyLayerIndex < 32)
        {
            Physics2D.IgnoreLayerCollision(playerLayerIndex, enemyLayerIndex, false);
        }
        else
        {
            try
            {
                int defaultPlayerLayer = LayerMask.NameToLayer("Player");
                int defaultEnemyLayer = LayerMask.NameToLayer("Enemy");

                if (defaultPlayerLayer != -1 && defaultEnemyLayer != -1)
                {
                    Physics2D.IgnoreLayerCollision(defaultPlayerLayer, defaultEnemyLayer, false);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DASH] Error restoring collisions: {e.Message}");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDashing) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
        OnTakeDamage?.Invoke(this, EventArgs.Empty);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    private void SetHeartImagesSortingLayer()
    {
        if (heartImages == null || heartImages.Count == 0) return;

        foreach (var heartImage in heartImages)
        {
            if (heartImage == null) continue;
            
            Canvas canvas = heartImage.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingLayerName = healthUISortingLayerName;
                canvas.sortingOrder = healthUISortingOrder;
            }
        }
    }

    private void UpdateHealthUI()
    {
        if (heartImages == null || heartImages.Count == 0) return;

        int fullHearts = currentHealth / 10;
        bool hasPartialHeart = (currentHealth % 10) > 0;

        for (int i = 0; i < heartImages.Count; i++)
        {
            if (heartImages[i] == null) continue;

            if (i < fullHearts)
            {
                heartImages[i].sprite = fullHeartSprite;
                heartImages[i].enabled = true;
            }
            else if (i == fullHearts && hasPartialHeart)
            {
                heartImages[i].sprite = emptyHeartSprite;
                heartImages[i].enabled = true;
            }
            else
            {
                heartImages[i].enabled = false;
            }
        }
    }

    private void Die()
    {
        OnDeath?.Invoke(this, EventArgs.Empty);
        enabled = false;
        rb.linearVelocity = Vector2.zero;
    }

    private void ShowBow()
    {
        if (bowInstance != null) return;

        if (bowPrefab != null)
        {
            Transform parent = bowParent != null ? bowParent : transform;
            bowInstance = Instantiate(bowPrefab, parent.position, Quaternion.identity, parent);
            arrowSpawnPoint = bowInstance.transform.Find("ArrowSpawnPoint");
            isBowActive = true;
        }
    }

    private void HideBow()
    {
        if (bowInstance != null)
        {
            Destroy(bowInstance);
            bowInstance = null;
            isBowActive = false;
        }
    }

    private void ShootArrow()
    {
        if (bowInstance == null || arrowPrefab == null || arrowSpawnPoint == null) return;

        Vector2 spawnPos = arrowSpawnPoint.position;
        GameObject arrowObj = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);

        MonoBehaviour[] components = arrowObj.GetComponents<MonoBehaviour>();
        foreach (var component in components)
        {
            System.Reflection.MethodInfo methodInfo = component.GetType().GetMethod("ShootArrow");
            if (methodInfo != null)
            {
                methodInfo.Invoke(component, new object[] { bowDirection });
                break;
            }
        }
    }

    private void UpdateBowDirection()
    {
        Vector2 mousePosition = GameInput.Instance.GetMouseWorldPosition();
        bowDirection = (mousePosition - (Vector2)transform.position).normalized;
        bowInstance.transform.position = (Vector2)transform.position + bowDirection * bowOffset;
    }

    private void RotateBowTowardsMouse()
    {
        float angle = Mathf.Atan2(bowDirection.y, bowDirection.x) * Mathf.Rad2Deg;
        angle -= 180f;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
        bowInstance.transform.rotation = Quaternion.Slerp(
            bowInstance.transform.rotation,
            targetRotation,
            bowRotationSpeed * Time.deltaTime
        );
    }

    public bool IsRunning() => isRunning;
    public bool IsDashing() => isDashing;
    public bool IsBowActive() => isBowActive;
    public Vector3 GetPlayerScreenPosition() => Camera.main.WorldToScreenPoint(transform.position);
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;

    public void LoadHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();
    }

    [ContextMenu("Test Take 10 Damage")]
    private void TestTakeDamage()
    {
        TakeDamage(10);
    }

    [ContextMenu("Test Heal 10 Health")]
    private void TestHeal()
    {
        Heal(10);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<EnemyEntity>() != null && isDashing)
        {
            Debug.LogWarning($"[PLAYER] Player collided with enemy during dash!");
        }
    }
}