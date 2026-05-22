using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Ținte și Mișcare")]
    public Transform player;
    public float detectionRange = 8f;
    public float patrolRadius = 5f;

    [Header("Setări Animație")]
    public float animationSmoothSpeed = 8f;

    private GameObject patrolTargetObject;
    private Component aiDestinationSetter;
    private Animator anim;
    private Component aiPathHidden;

    [Header("Setări Perseverență")]
    public float chasePersistence = 3f;
    private float persistenceTimer;
    private bool isChasing = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        aiDestinationSetter = GetComponent("AIDestinationSetter");
        aiPathHidden = GetComponent("AIPath");

        patrolTargetObject = new GameObject("Urs_PatrolTarget");
        GetNewPatrolPoint();
    }

    void Update()
    {
        if (player == null || aiDestinationSetter == null) return;

        // 1. Verificăm SafeZone 
        var playerScript = player.GetComponent<PlayerMovement>();
        bool playerSafe = (playerScript != null) ? playerScript.isInSafeZone : false;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 2. Logică de Urmărire
        if (distanceToPlayer <= detectionRange && !playerSafe)
        {
            isChasing = true;
            persistenceTimer = chasePersistence;
        }
        else
        {
            persistenceTimer -= Time.deltaTime;
            if (persistenceTimer <= 0 || playerSafe) isChasing = false;
        }

        // 3. Setăm ținta
        if (isChasing) SetAITarget(player);
        else Patrol();

        // 4. Actualizăm Animația
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (aiPathHidden == null || anim == null) return;

        // Luăm VITEZA reală a ursului din A* Pathfinding în loc de direcția țintei
        var velocityProperty = aiPathHidden.GetType().GetProperty("velocity");
        if (velocityProperty != null)
        {
            Vector3 velocity = (Vector3)velocityProperty.GetValue(aiPathHidden, null);

            // Dacă ursul chiar se mișcă (nu stă pe loc)
            if (velocity.magnitude > 0.1f)
            {
                Vector2 targetDir = new Vector2(velocity.x, velocity.y).normalized;

                // Citim valorile actuale din Animator (Acum folosim X și Y)
                float currentX = anim.GetFloat("X");
                float currentY = anim.GetFloat("Y");

                // Smoothing pentru ca ursul să nu se întoarcă brusc
                float smoothX = Mathf.MoveTowards(currentX, targetDir.x, Time.deltaTime * animationSmoothSpeed);
                float smoothY = Mathf.MoveTowards(currentY, targetDir.y, Time.deltaTime * animationSmoothSpeed);

                // Trimitem noile valori către Blend Tree
                anim.SetFloat("X", smoothX);
                anim.SetFloat("Y", smoothY);
                anim.SetBool("isWalking", true);
            }
            else
            {
                // Când se oprește, oprim animația de mers (dar își păstrează ultima privire)
                anim.SetBool("isWalking", false);
            }
        }
    }

    void Patrol()
    {
        if (Vector2.Distance(transform.position, patrolTargetObject.transform.position) < 0.5f)
            GetNewPatrolPoint();

        SetAITarget(patrolTargetObject.transform);
    }

    void GetNewPatrolPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 potentialPoint = (Vector2)transform.position + Random.insideUnitCircle * patrolRadius;
            Collider2D hit = Physics2D.OverlapPoint(potentialPoint);

            if (hit == null || (!hit.CompareTag("SafeZone") && hit.gameObject.layer != LayerMask.NameToLayer("Obstacole")))
            {
                patrolTargetObject.transform.position = potentialPoint;
                return;
            }
        }
        patrolTargetObject.transform.position = transform.position;
    }

    void SetAITarget(Transform targetTransform)
    {
        if (aiDestinationSetter != null)
        {
            var targetField = aiDestinationSetter.GetType().GetField("target");
            if (targetField != null) targetField.SetValue(aiDestinationSetter, targetTransform);
        }
    }
}