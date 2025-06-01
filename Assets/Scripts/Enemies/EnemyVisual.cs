using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVisual : MonoBehaviour {

    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private EnemyEntity enemyEntity;
    private Animator animator;

    private const string IS_RUNNING = "isRunning";
    private const string CHASING_SPEED_MULTIPLIER = "ChasingSpeedMultiplier";
    private const string ATTACK = "Attack";    private const string DEATH = "isDie";

    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private GameObject attackEffectPrefab;

    private void Awake() 
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        enemyAI.OnEnemyAttack += enemyAI_OnEnemyAttack;
        enemyEntity.OnEnemyDeath += enemyEntity_OnEnemyDeath;
    }

    private void OnDestroy()
    {
        enemyAI.OnEnemyAttack -= enemyAI_OnEnemyAttack;
        enemyEntity.OnEnemyDeath -= enemyEntity_OnEnemyDeath;
    }
    private void enemyEntity_OnEnemyDeath(object sender, System.EventArgs e)
    {
        Debug.Log("[ENEMY] Playing Death Animation");

        animator.ResetTrigger(ATTACK);

        animator.SetBool(IS_RUNNING, false);

        animator.SetBool(DEATH, true);

        Debug.Log($"[ENEMY] Death parameter set: {animator.GetBool(DEATH)}");

        AnimatorControllerParameter[] parameters = animator.parameters;
        string paramsList = "Параметры аниматора: ";
        foreach (var param in parameters)
        {
            paramsList += $"{param.name} ({param.type}), ";
        }
        Debug.Log(paramsList);

        try
        {
            animator.Play("Death");
            Debug.Log("[ENEMY] Forced Death animation state");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[ENEMY] Could not force Death state: {ex.Message}");
        }

        StartCoroutine(WaitForDeathAnimation());
    }
    private IEnumerator WaitForDeathAnimation()
    {
        float deathAnimationLength = enemyEntity.GetDeathAnimationLength();
        Debug.Log($"[ENEMY] Waiting for death animation to complete. Animation length: {deathAnimationLength} seconds");

        yield return new WaitForSeconds(0.1f);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"[ENEMY] Animator state after 0.1s: {stateInfo.fullPathHash}, normalized time: {stateInfo.normalizedTime}, isDie param: {animator.GetBool(DEATH)}");

        for (float t = 0; t < deathAnimationLength; t += 0.5f)
        {
            yield return new WaitForSeconds(0.5f);
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[ENEMY] Animator state at {t}s: {stateInfo.fullPathHash}, normalized time: {stateInfo.normalizedTime}");

            if (!stateInfo.IsName("Death") && !stateInfo.IsName("Base Layer.Death"))
            {
                Debug.LogWarning("[ENEMY] Not in death animation, forcing it again");
                animator.SetBool(DEATH, true);
                try
                {
                    animator.Play("Death");
                }
                catch { }
            }
        }
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"[ENEMY] Animator state after waiting: {stateInfo.fullPathHash}, normalized time: {stateInfo.normalizedTime}, isDie param: {animator.GetBool(DEATH)}");

        Debug.Log("[ENEMY] Death animation completed");
    }
    private void Update()
    {
        if (enemyEntity.IsDead())
        {
            animator.SetBool(DEATH, true);
            
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[ENEMY DEATH DEBUG] Current animator state: {stateInfo.fullPathHash}, isDie: {animator.GetBool(DEATH)}, normalized time: {stateInfo.normalizedTime}");
        }
        else
        {
            animator.SetBool(IS_RUNNING, enemyAI.IsRunning);
            animator.SetFloat(CHASING_SPEED_MULTIPLIER, enemyAI.GetRoamingSpeed());
        }
    }
    public void TriggerAttackTurnOff()
    {
        enemyEntity.PolygonColliderTurnOff();
    }
    
    public void TriggerAttackTurnOn()
    {
        enemyEntity.PolygonColliderTurnOn();
    }
    
    public void TriggerAttackDamage()
    {
        Debug.Log("[ANIMATION EVENT] TriggerAttackDamage called from animation event!");
        DealDamageToPlayer();
    }
      private void enemyAI_OnEnemyAttack(object sender, System.EventArgs e)
    {
        Debug.Log("[GOBLIN] Starting attack animation");
        animator.SetTrigger(ATTACK);
    }
      public void DealDamageToPlayer()
    {
        if (Player.Instance == null) 
        {
            Debug.LogError("Player.Instance is null! Cannot deal damage.");
            return;
        }
        float distanceToPlayer = Vector2.Distance(transform.position, Player.Instance.transform.position);
        
        Debug.Log($"[GOBLIN ATTACK] Attack triggered from Animation Event. Distance: {distanceToPlayer}, Attack range: {attackRange}");
          float effectiveRange = attackRange * 1.5f;
        
        if (distanceToPlayer <= effectiveRange)
        {
            Player.Instance.TakeDamage(attackDamage);
            Debug.Log($"[GOBLIN ATTACK] Goblin dealt {attackDamage} damage to player! Distance: {distanceToPlayer}");
            if (attackEffectPrefab != null)
            {
                Instantiate(
                    attackEffectPrefab, 
                    Player.Instance.transform.position, 
                    Quaternion.identity
                );
            }
        }
        else
        {
            Debug.Log($"[GOBLIN ATTACK] Player out of attack range. Distance: {distanceToPlayer}, Range: {effectiveRange}");
        }
    }
}
