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

        // 2. NOU: Verificăm dacă jucătorul este mort
        var playerStats = player.GetComponent<PlayerStats>();
        bool playerDead = (playerStats != null) ? playerStats.isDead : false;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 3. Logică de Urmărire (Te atacă doar dacă NU ești safe și NU ești mort)
        if (distanceToPlayer <= detectionRange && !playerSafe && !playerDead)
        {
            isChasing = true;
            persistenceTimer = chasePersistence;
        }
        else
        {
            persistenceTimer -= Time.deltaTime;
            // Dacă cronometrul a expirat, sau ești safe, sau ai murit -> se întoarce la patrulare
            if (persistenceTimer <= 0 || playerSafe || playerDead) isChasing = false;
        }

        // 4. Setăm ținta
        if (isChasing) SetAITarget(player);
        else Patrol();

        // 5. Actualizăm Animația
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (aiPathHidden == null || anim == null) return;

        var targetProperty = aiPathHidden.GetType().GetProperty("steeringTarget");
        if (targetProperty != null)
        {
            Vector3 steeringTarget = (Vector3)targetProperty.GetValue(aiPathHidden, null);
            Vector3 rawDirection = steeringTarget - transform.position;

            if (rawDirection.magnitude > 0.1f)
            {
                Vector2 targetDir = new Vector2(rawDirection.x, rawDirection.y).normalized;

                float currentH = anim.GetFloat("Horizontal");
                float currentV = anim.GetFloat("Vertical");

                // Smoothing pentru mișcare lină în Blend Tree
                float smoothH = Mathf.MoveTowards(currentH, targetDir.x, Time.deltaTime * animationSmoothSpeed);
                float smoothV = Mathf.MoveTowards(currentV, targetDir.y, Time.deltaTime * animationSmoothSpeed);

                anim.SetFloat("Horizontal", smoothH);
                anim.SetFloat("Vertical", smoothV);
                anim.SetBool("isWalking", true);
            }
            else
            {
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