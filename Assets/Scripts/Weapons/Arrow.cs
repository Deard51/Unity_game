using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float arrowSpeed = 15f;
    [SerializeField] private int arrowDamage = 10;
    [SerializeField] private float maxLifetime = 5f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, maxLifetime);
    }    public void ShootArrow(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle -= 90f;
        
        transform.rotation = Quaternion.Euler(0, 0, angle);
        rb.linearVelocity = direction.normalized * arrowSpeed;
    }
    
    public int GetArrowDamage()
    {
        return arrowDamage;
    }    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (other.TryGetComponent<EnemyEntity>(out EnemyEntity enemy))
        {
            enemy.TakeDamage(arrowDamage);
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
