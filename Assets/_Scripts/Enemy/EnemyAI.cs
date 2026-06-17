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

    [Header("Setări Perseverență & Anti-Blocare")]
    public float chasePersistence = 3f;
    private float persistenceTimer;
    private bool isChasing = false;

    public float maxPatrolTime = 4f; // Cât timp încearcă să ajungă la un punct
    private float patrolTimer = 0f;

    // --- VARIABILE NOI PENTRU SUNET URS ---
    [Header("Setări Sunet Urs")]
    private float roarTimer = 0f;
    public float roarInterval = 3f;          // La câte secunde să urle în chase
    private bool wasChasingLastFrame = false;  // Să știe când începe fix atacul
    // --------------------------------------

    void Start()
    {
        anim = GetComponent<Animator>();
        aiDestinationSetter = GetComponent("AIDestinationSetter");
        aiPathHidden = GetComponent("AIPath");

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        patrolTargetObject = new GameObject("Urs_PatrolTarget_" + gameObject.GetInstanceID());
        GetNewPatrolPoint(false); // La început, alege un punct normal
    }

    void Update()
    {
        if (player == null || aiDestinationSetter == null) return;

        var playerScript = player.GetComponent<PlayerMovement>();
        bool playerSafe = (playerScript != null) ? playerScript.isInSafeZone : false;

        var playerStats = player.GetComponent<PlayerStats>();
        bool playerDead = (playerStats != null) ? playerStats.isDead : false;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange && !playerSafe && !playerDead)
        {
            isChasing = true;
            persistenceTimer = chasePersistence;
        }
        else
        {
            persistenceTimer -= Time.deltaTime;
            if (persistenceTimer <= 0 || playerSafe || playerDead) isChasing = false;
        }

        if (isChasing)
        {
            SetAITarget(player);
            patrolTimer = 0f; // Resetăm timer-ul de blocare cât timp fuge după tine

            // --- COD NOU PENTRU SUNET (ROAR) AICI ---
            // 1. Când abia te-a văzut și începe chase-ul
            if (!wasChasingLastFrame)
            {
                // Asigură-te că ai un sunet numit exact "BearRoar" în lista din AudioManager
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.PlaySound("BearRoar");
                }
                roarTimer = roarInterval;
                wasChasingLastFrame = true;
            }

            // 2. Scădem timpul și urlă iar dacă a trecut intervalul (ex: 3 sec)
            roarTimer -= Time.deltaTime;
            if (roarTimer <= 0f)
            {
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.PlaySound("BearRoar");
                }
                roarTimer = roarInterval;
            }
            // ------------------------------------------
        }
        else
        {
            Patrol();

            // --- Când pierde jucătorul, se calmează ---
            wasChasingLastFrame = false;
        }

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
        patrolTimer += Time.deltaTime;

        // 1. Dacă a ajuns la destinație -> Alege un punct nou normal
        if (Vector2.Distance(transform.position, patrolTargetObject.transform.position) < 0.5f)
        {
            GetNewPatrolPoint(false);
        }
        // 2. Dacă s-a blocat în gard/margine (a expirat timerul) -> Forțează întoarcerea!
        else if (patrolTimer > maxPatrolTime)
        {
            GetNewPatrolPoint(true);
        }

        SetAITarget(patrolTargetObject.transform);
    }

    void GetNewPatrolPoint(bool isStuck)
    {
        patrolTimer = 0f;

        // SISTEM NOU: Dacă e blocat la margine, forțează-l să meargă înspre zona activă a hărții (spre player)
        if (isStuck && player != null)
        {
            Vector2 directionTowardsCenter = (player.position - transform.position).normalized;

            // Îi dăm un unghi random (-45 la 45 grade) ca să nu meargă toți urșii lipiți unul de altul direct spre tine
            float randomAngle = Random.Range(-45f, 45f);
            Vector2 directionWithVariation = Quaternion.Euler(0, 0, randomAngle) * directionTowardsCenter;

            patrolTargetObject.transform.position = (Vector2)transform.position + directionWithVariation * patrolRadius;
            return;
        }

        // Dacă nu e blocat, comportament normal de patrulare
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