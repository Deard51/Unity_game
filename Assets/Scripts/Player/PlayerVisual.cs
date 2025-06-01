using UnityEngine;
using System.Collections;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private GameObject attackEffectPrefab;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackEffectDuration = 0.2f;
    
    [SerializeField] private Color dashColor = new Color(1f, 1f, 1f, 0.5f);
    
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Color originalColor;
    private readonly string IS_RUNNING = "IsRunning";
    private readonly string ATTACK_TRIGGER = "Attack";
    private readonly string TAKE_DAMAGE = "TakeHit";
    private readonly string DIE = "isDie";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        if (Player.Instance != null)
        {
            Player.Instance.OnAttack += Player_OnAttack;
            Player.Instance.OnDash += Player_OnDash;
            Player.Instance.OnTakeDamage += Player_OnTakeDamage;
            Player.Instance.OnDeath += Player_OnDeath;
        }
        else
        {
            Debug.LogError("Player.Instance не найден!");
        }
    }

    private void OnDestroy()
    {
        if (Player.Instance != null)
        {
            Player.Instance.OnAttack -= Player_OnAttack;
            Player.Instance.OnDash -= Player_OnDash;
            Player.Instance.OnTakeDamage -= Player_OnTakeDamage;
            Player.Instance.OnDeath -= Player_OnDeath;
        }    }
    
    private bool HasParameter(string paramName)
    {
        if (animator == null)
            return false;
            
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
    
    private void Update()
    {
        if (animator != null && Player.Instance != null)
        {
            if (HasParameter(IS_RUNNING))
            {
                animator.SetBool(IS_RUNNING, Player.Instance.IsRunning());
            }
        }
        
        AdjustPlayerFacingDirection();
    }

    private void AdjustPlayerFacingDirection()
    {
        if (GameInput.Instance == null) return;
        
        Vector3 mousePos = GameInput.Instance.GetMousePosition();
        Vector3 playerPosition = Player.Instance.GetPlayerScreenPosition();

        if (mousePos.x < playerPosition.x)
        {
            spriteRenderer.flipX = true;
            
            if (attackPoint != null)
            {
                Vector3 localPos = attackPoint.localPosition;
                attackPoint.localPosition = new Vector3(-Mathf.Abs(localPos.x), localPos.y, localPos.z);
            }
        }
        else
        {
            spriteRenderer.flipX = false;
            if (attackPoint != null)
            {
                Vector3 localPos = attackPoint.localPosition;
                attackPoint.localPosition = new Vector3(Mathf.Abs(localPos.x), localPos.y, localPos.z);
            }
        }
    }    private void Player_OnAttack(object sender, System.EventArgs e)
    {
        Debug.Log("PlayerVisual: Player_OnAttack event received!");
        
        if (HasParameter(ATTACK_TRIGGER))
        {
            Debug.Log($"Setting animation trigger: {ATTACK_TRIGGER}");
            animator.SetTrigger(ATTACK_TRIGGER);
        }
        else
        {
            Debug.LogError($"Animation parameter '{ATTACK_TRIGGER}' not found in animator!");
        }
        
        if (attackEffectPrefab != null && attackPoint != null)
        {
            Debug.Log("Creating attack effect");
            GameObject effect = Instantiate(
                attackEffectPrefab,
                attackPoint.position,
                Quaternion.identity
            );
            Destroy(effect, attackEffectDuration);
        }
    }
    
    private void Player_OnDash(object sender, System.EventArgs e)
    {
        StartCoroutine(DashVisualEffect());
    }
    
    private IEnumerator DashVisualEffect()
    {
        spriteRenderer.color = dashColor;
        
        yield return new WaitForSeconds(0.2f);
        
        spriteRenderer.color = originalColor;
    }
    
    private void Player_OnTakeDamage(object sender, System.EventArgs e)
    {
        animator.SetTrigger(TAKE_DAMAGE);
        StartCoroutine(DamageFlashEffect());
    }
    
    private IEnumerator DamageFlashEffect()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
      private void Player_OnDeath(object sender, System.EventArgs e)
    {
        animator.SetTrigger(DIE);
    }
    public void PlayerAttackEvent()
    {
        Debug.Log("Attack animation event triggered");
        if (Player.Instance != null)
        {
            Player.Instance.DealDamage();
        }
    }

    public void EnableAttackCollider()
    {
        Debug.Log("Animation event: EnableAttackCollider called");
        if (Player.Instance != null)
        {
            Player.Instance.AttackColliderTurnOn();
        }
    }
    
    public void DisableAttackCollider()
    {
        Debug.Log("Animation event: DisableAttackCollider called");
        if (Player.Instance != null)
        {
            Player.Instance.AttackColliderTurnOff();
        }
    }
}
