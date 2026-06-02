using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Setări Critice")]
    [Range(0, 100)]
    public int sansaCritica = 20;
    public float multiplicatorCritic = 2.0f;

    [Header("Setări Atac & Cooldown")]
    public KeyCode attackKey = KeyCode.Space;
    public float attackRange = 1.0f;
    public float attackCooldown = 1.0f; // NOU: Cooldown de 1 secundă între atacuri
    private float nextAttackTime = 0f;   // NOU: Cronometrul intern pentru cooldown

    [Range(0f, 360f)]
    public float attackAngle = 120f;

    [Range(0f, 1f)]
    public float attackOffset = 0.3f;

    [Header("Referințe")]
    public Animator swordAnimator;
    public Transform swordSlashTransform;
    public PlayerMovement movement;
    public DamagePopup damagePopupPrefab;

    private PlayerStats playerStats;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();

        if (swordSlashTransform != null)
        {
            swordSlashTransform.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // NOU: Verificăm dacă a trecut timpul de cooldown înainte de a putea ataca din nou
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetKeyDown(attackKey))
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown; // Blocăm atacul pentru următoarea secundă
            }
        }
    }

    void Attack()
    {
        if (movement == null || swordAnimator == null || swordSlashTransform == null) return;

        bool areArma = EquipmentManager.instance.AreArmaEchipata();

        if (areArma)
        {
            swordSlashTransform.gameObject.SetActive(true);
            PositionSlash();
            swordAnimator.SetTrigger("Attack");
        }
        else
        {
            swordSlashTransform.gameObject.SetActive(false);
            Debug.Log("Ataci cu pumnul! Nu ai sabia echipată pe primul slot.");
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);

        int damageArma = EquipmentManager.instance.GetDamageCurent();
        int bonusStrength = 0;

        if (playerStats != null)
        {
            bonusStrength = playerStats.strength * 2;
        }

        int actualDamage = damageArma + bonusStrength;

        int sansaTotalaCritica = sansaCritica;

        if (playerStats != null)
        {
            sansaTotalaCritica += playerStats.dexterity;
        }

        bool esteCritic = Random.Range(1, 101) <= sansaTotalaCritica;
        int finalDamage = actualDamage;

        if (esteCritic)
        {
            finalDamage = Mathf.RoundToInt(actualDamage * multiplicatorCritic);
        }

        bool amLovitMacarUnInamic = false;

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyStats stats = enemy.GetComponent<EnemyStats>();

            if (stats != null)
            {
                Vector2 directionToEnemy = (enemy.transform.position - transform.position).normalized;
                float angle = Vector2.Angle(movement.lastMoveDir, directionToEnemy);

                if (angle < attackAngle / 2f)
                {
                    stats.TakeDamage(finalDamage);

                    // NOU: Îi aplicăm inamicului efectul de freeze (înghețare) pentru 0.3 secunde
                    stats.ApplyHitStun(0.3f);

                    amLovitMacarUnInamic = true;
                }
            }
        }

        if (amLovitMacarUnInamic && damagePopupPrefab != null)
        {
            Vector3 pozitieText = transform.position + new Vector3(0, 1f, 0);
            DamagePopup popup = Instantiate(damagePopupPrefab, pozitieText, Quaternion.identity);
            popup.SeteazaDamage(finalDamage, esteCritic);
        }
    }

    void PositionSlash()
    {
        Vector2 dir = movement.lastMoveDir;
        swordSlashTransform.localRotation = Quaternion.identity;
        swordSlashTransform.localScale = new Vector3(1, 1, 1);

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x > 0)
            {
                swordSlashTransform.localPosition = new Vector3(attackOffset, 0, 0);
                swordSlashTransform.localEulerAngles = new Vector3(0, 0, 0);
            }
            else
            {
                swordSlashTransform.localPosition = new Vector3(-attackOffset, 0, 0);
                swordSlashTransform.localEulerAngles = new Vector3(0, 0, 180f);
                swordSlashTransform.localScale = new Vector3(1, -1, 1);
            }
        }
        else
        {
            if (dir.y > 0)
            {
                swordSlashTransform.localPosition = new Vector3(0, attackOffset, 0);
                swordSlashTransform.localEulerAngles = new Vector3(0, 0, 90f);
            }
            else
            {
                swordSlashTransform.localPosition = new Vector3(0, -attackOffset, 0);
                swordSlashTransform.localEulerAngles = new Vector3(0, 0, -90f);
                swordSlashTransform.localScale = new Vector3(1, -1, 1);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}