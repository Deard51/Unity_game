using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyEntity : MonoBehaviour 
{    [SerializeField] private int maxHealth;
    private int currentHealth;
    private PolygonCollider2D polygonCollider2D;
    private Rigidbody2D rb;
    private bool isDead = false;
    [SerializeField] private float corpseRemainTime = 5f;
    [SerializeField] private float deathAnimationLength = 1.0f;
    
    private Animator animator;
    
    public event EventHandler OnEnemyDeath;    private void Awake()
    {
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        animator = GetComponentInChildren<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("[ENEMY ENTITY] Animator not found! Death animation will not work.");
        }
        else
        {
            Debug.Log("[ENEMY ENTITY] Animator found. Ready to play animations.");
        }
        
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.useFullKinematicContacts = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }    private void Start() 
    {
        currentHealth = maxHealth;
        
        GameObject enemyManagerObject = GameObject.Find("EnemyManager");
        if (enemyManagerObject != null)
        {
            var manager = enemyManagerObject.GetComponent<MonoBehaviour>();
            if (manager != null)
            {
                Debug.Log("Enemy manager found, will handle enemy respawn");
            }
        }
    }

    public void TakeDamage(int damage) 
    {
        if (isDead) return;
        
        currentHealth -= damage;

        DetectDeath(); 
    }    private void DetectDeath() 
    {
        if (currentHealth <= 0 && !isDead) 
        {
            Debug.Log("[ENEMY ENTITY] Enemy is dead! Health: " + currentHealth);
            isDead = true;
            
            animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                Debug.Log("[ENEMY ENTITY] Setting isDie parameter to true");
                animator.SetBool("isDie", true);
                
                Debug.Log($"[ENEMY ENTITY] isDie parameter value after setting: {animator.GetBool("isDie")}");
                
                animator.ResetTrigger("Attack");
                if (animator.GetBool("isRunning"))
                {
                    animator.SetBool("isRunning", false);
                }
            }
            else
            {
                Debug.LogError("[ENEMY ENTITY] No animator found in children!");
            }
            
            OnEnemyDeath?.Invoke(this, EventArgs.Empty);
            
            DisableEnemyComponents();
            
            StartCoroutine(DestroyAfterDelay());
        }
    }
    
    private void DisableEnemyComponents()
    {
        if (polygonCollider2D != null)
            polygonCollider2D.enabled = false;
        
        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
            enemyAI.SetDeathState();
        
        if (rb != null)
            rb.simulated = false;
    }    private IEnumerator DestroyAfterDelay()
    {
        Debug.Log($"[ENEMY ENTITY] Started DestroyAfterDelay coroutine. Will destroy in {corpseRemainTime} seconds");
        
        yield return new WaitForSeconds(0.2f);
        
        if (animator != null)
        {
            Debug.Log($"[ENEMY ENTITY] Animator found. isDie parameter value: {animator.GetBool("isDie")}");
            
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            string stateName = "Unknown";
            
            if (stateInfo.IsName("Death")) stateName = "Death";
            else if (stateInfo.IsName("Base Layer.Death")) stateName = "Base Layer.Death";
            
            Debug.Log($"[ENEMY ENTITY] Current animator state: {stateInfo.shortNameHash}, normalized time: {stateInfo.normalizedTime}, might be: {stateName}");
            
            if (deathAnimationLength > 0)
            {
                Debug.Log($"[ENEMY ENTITY] Waiting for death animation to complete: {deathAnimationLength} seconds");
                yield return new WaitForSeconds(deathAnimationLength);
            }
        }
        
        Debug.Log($"[ENEMY ENTITY] Death animation completed. Keeping corpse for {corpseRemainTime} seconds");
        yield return new WaitForSeconds(corpseRemainTime);
        
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[ENEMY ENTITY] Before destroy - Animator state: {stateInfo.shortNameHash}, isDie: {animator.GetBool("isDie")}");
        }
        
        Debug.Log($"[ENEMY ENTITY] Destroying enemy game object");
        Destroy(gameObject);
    }
      public void PolygonColliderTurnOff()
    {
        polygonCollider2D.enabled = false;
    }

    public void PolygonColliderTurnOn()
    {
        if (!isDead)
            polygonCollider2D.enabled = true;
    }
    public bool IsDead()
    {
        return isDead;
    }
    
    public float GetDeathAnimationLength()
    {
        return deathAnimationLength;
    }
}
