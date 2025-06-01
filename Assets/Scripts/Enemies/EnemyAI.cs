using UnityEngine;
using UnityEngine.AI;
using System;

public class EnemyAI : MonoBehaviour {

    [SerializeField] private State startingState;
    [SerializeField] private float roamingDistanceMax = 7f;
    [SerializeField] private float roamimgDistanceMin = 3f;
    [SerializeField] private float roamimgTimerMax = 2f;

    [SerializeField] private bool isChasingEnemy = false;
    [SerializeField] private float chasingDistance = 4f;
    [SerializeField] private float chasingSpeedMultiplier = 2f;    [SerializeField] private bool isAttackingEnemy = false;
    [SerializeField] private float attackingDistance = 1.5f;
    [SerializeField] private float attackRate = 2f;
    private float nextAttackTime = 0f;

    private NavMeshAgent navMeshAgent;
    private State currentState;
    private float roamingTimer;
    private Vector3 roamPosition;
    private Vector3 startingPosition;

    private float roamingSpeed;
    private float chasingSpeed;

    private float nextCheckDirectionTime = 0f;
    private float checkDirectionDuration = 0.1f;
    private Vector3 lastPosition;

    public event EventHandler OnEnemyAttack;


    public bool IsRunning 
    {
        get 
        {
            if (navMeshAgent.velocity == Vector3.zero) 
            {
                return false;
            } 
            else 
            {
                return true;
            }
        }
    }


    private enum State 
    {
        Idle,
        Roaming,
        Chasing,
        Attacking,
        Death
    }

    private void Awake() 
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        currentState = startingState;

        roamingSpeed = navMeshAgent.speed;
        chasingSpeed = navMeshAgent.speed * chasingSpeedMultiplier;
    }


    private void Update() 
    {
        StateHandler();
        MovementDirectionHandler();
    }    private void StateHandler() 
    {
        if (currentState == State.Death)
            return;
            
        switch (currentState) 
        {
            case State.Roaming:
                roamingTimer -= Time.deltaTime;
                if (roamingTimer < 0) 
                {
                    Roaming();
                    roamingTimer = roamimgTimerMax;
                }
                CheckCurrentState();
                break;
            case State.Chasing:
                ChasingTarget();
                CheckCurrentState();
                break;
            case State.Attacking:
                AttackingTarget();
                CheckCurrentState();
                break;            case State.Death:
                break;
            default:
            case State.Idle:
                break;
        }
    }

    private void ChasingTarget() 
    {
        navMeshAgent.SetDestination(Player.Instance.transform.position);
    }

    public float GetRoamingSpeed() 
    {
        return navMeshAgent.speed / roamingSpeed;
    }    private void CheckCurrentState() 
    {
        if (currentState == State.Death)
            return;
            
        float distanceToPlayer = Vector3.Distance(transform.position, Player.Instance.transform.position);
        State newState = State.Roaming;

        if (isChasingEnemy) 
        {
            if (distanceToPlayer <= chasingDistance) 
            {
                newState = State.Chasing;
            }
        }

        if (isAttackingEnemy) 
        {
            if (distanceToPlayer <= attackingDistance) 
            {
                newState = State.Attacking;
            }
        }

        if (newState != currentState) 
        {
            if (newState == State.Chasing) 
            {
                navMeshAgent.ResetPath();
                navMeshAgent.speed = chasingSpeed;
            } 
            else if (newState == State.Roaming) 
            {
                roamingTimer = 0f;
                navMeshAgent.speed = roamingSpeed;
            } 
            else if (newState == State.Attacking) 
            {
                navMeshAgent.ResetPath();
            }

            currentState = newState;
        }
    }
    private void AttackingTarget() 
    {
        if (Time.time > nextAttackTime) 
        {
            Debug.Log($"[ENEMY AI] Starting attack. Distance to player: {Vector3.Distance(transform.position, Player.Instance.transform.position)}");
            OnEnemyAttack?.Invoke(this, EventArgs.Empty);

            nextAttackTime = Time.time + attackRate;
        }
    }    private void MovementDirectionHandler() 
    {
        if (currentState == State.Death)
            return;
            
        if (Time.time > nextCheckDirectionTime) 
        {
            if (IsRunning) 
            {
                ChangeFacingDirection(lastPosition, transform.position);
            } 
            else if (currentState == State.Attacking) 
            {
                ChangeFacingDirection(transform.position, Player.Instance.transform.position);
            }

            lastPosition = transform.position;
            nextCheckDirectionTime = Time.time + checkDirectionDuration;
        }
    }    public void SetDeathState()
    {        Debug.Log($"[ENEMY AI] Changing state from {currentState} to Death");
        currentState = State.Death;
        
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.ResetPath();
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }
          Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            Debug.Log("[ENEMY AI] Found animator component, checking parameters");
            
            animator.SetBool("isDie", true);
            
            Debug.Log($"[ENEMY AI] isDie parameter value: {animator.GetBool("isDie")}");
        }
        else
        {
            Debug.LogWarning("[ENEMY AI] No animator component found!");
        }
    }


    private void Roaming() 
    {
        startingPosition = transform.position;
        roamPosition = GetRoamingPosition();
        navMeshAgent.SetDestination(roamPosition);
    }



    private Vector3 GetRoamingPosition() 
    {
        return startingPosition + GetRandomDir() * UnityEngine.Random.Range(roamimgDistanceMin, roamingDistanceMax);
    }

    private void ChangeFacingDirection(Vector3 sourcePosition, Vector3 targetPosition) 
    {
        if (sourcePosition.x > targetPosition.x) 
        {
            transform.rotation = Quaternion.Euler(0, -180, 0);
        } 
        else 
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
    public static Vector3 GetRandomDir()
    {
        return new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
    }
}
