using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Setări Mișcare")]
    public float speed = 2f;
    public float chaseSpeed = 3.5f;
    public float detectionRange = 5f;
    public float stopDistance = 1.2f;

    [Header("Patrol")]
    public float patrolRadius = 4f;
    private Vector2 patrolTarget;
    private float patrolTimer;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isChasing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Căutăm jucătorul după Tag. ASIGURĂ-TE CĂ PLAYERUL ARE TAG-UL "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        GetNewPatrolPoint();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Logică de detectare
        if (distanceToPlayer <= detectionRange) isChasing = true;
        else if (distanceToPlayer > detectionRange * 1.5f) isChasing = false;

        if (isChasing) ChasePlayer(distanceToPlayer);
        else Patrol();
    }

    void Move(Vector2 direction, float currentSpeed)
    {
        // Folosim .velocity pentru versiunile mai vechi de Unity
        rb.velocity = direction * currentSpeed;

        if (direction != Vector2.zero)
        {
            animator.SetFloat("Horizontal", direction.x);
            animator.SetFloat("Vertical", direction.y);
        }

        animator.SetFloat("Speed", direction.sqrMagnitude);
    }

    void ChasePlayer(float distance)
    {
        if (distance > stopDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            Move(direction, chaseSpeed);
        }
        else
        {
            Move(Vector2.zero, 0); // Se oprește lângă tine
            // Aici poți adăuga animator.SetTrigger("Attack") mai târziu
        }
    }

    void Patrol()
    {
        float distanceToPoint = Vector2.Distance(transform.position, patrolTarget);

        if (distanceToPoint < 0.2f || patrolTimer > 5f)
        {
            GetNewPatrolPoint();
            patrolTimer = 0;
        }

        Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
        Move(direction, speed);
        patrolTimer += Time.deltaTime;
    }

    void GetNewPatrolPoint()
    {
        patrolTarget = (Vector2)transform.position + Random.insideUnitCircle * patrolRadius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}