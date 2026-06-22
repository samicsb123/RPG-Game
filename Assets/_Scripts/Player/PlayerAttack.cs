using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    [Header("Setări Critice")]
    [Range(0, 100)]
    public int sansaCritica = 20;
    public float multiplicatorCritic = 2.0f;

    [Header("Setări Atac & Cooldown")]
    public KeyCode attackKey = KeyCode.Space;
    public float attackRange = 1.0f;
    public float attackCooldown = 1.0f;
    private float nextAttackTime = 0f;

    public float attackFreezeDuration = 0.3f;
    private float freezeTimer = 0f;
    private Rigidbody2D rb;

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
        // Ne asigurăm că scripturile sunt mereu găsite pe același obiect
        playerStats = GetComponent<PlayerStats>();
        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();

        // Protecții pentru a depista ușor ce ai uitat să pui în Inspector
        if (swordSlashTransform == null)
        {
            Debug.LogError("ATENȚIE: Nu ai pus 'Sword Slash Transform' în Inspector la Player Attack!");
        }
        else
        {
            swordSlashTransform.gameObject.SetActive(false);
        }

        if (swordAnimator == null)
        {
            Debug.LogError("ATENȚIE: Nu ai pus 'Sword Animator' în Inspector la Player Attack!");
        }
    }

    void Update()
    {
        // 1. Verificăm dacă e mort sau dacă ceva vital lipsește
        if (playerStats != null && playerStats.isDead) return;
        if (movement == null) return;

        // 2. Cronometrul pentru blocarea mișcării
        if (freezeTimer > 0)
        {
            freezeTimer -= Time.deltaTime;
            // Dacă timpul s-a scurs, îi dăm înapoi dreptul de a se mișca
            if (freezeTimer <= 0)
            {
                movement.enabled = true;
            }
        }

        // 3. Verificăm dacă poate ataca (Cooldown)
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetKeyDown(attackKey))
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void Attack()
    {
        if (movement == null || swordAnimator == null || swordSlashTransform == null) return;

        // --- Înghețăm jucătorul pe loc în timpul atacului ---
        freezeTimer = attackFreezeDuration;
        movement.enabled = false;
        if (rb != null) rb.velocity = Vector2.zero;
        // ----------------------------------------------------

        // Protecție: Verificăm dacă EquipmentManager chiar există în scenă
        bool areArma = false;
        if (EquipmentManager.instance != null)
        {
            areArma = EquipmentManager.instance.AreArmaEchipata();
        }
        else
        {
            Debug.LogWarning("EquipmentManager lipsește din această scenă! Se presupune că nu ai armă.");
        }

        if (areArma)
        {
            swordSlashTransform.gameObject.SetActive(true);
            PositionSlash();
            swordAnimator.SetTrigger("Attack");

            // Protecție pentru AudioManager
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySound("Slash");
            }
        }
        else
        {
            swordSlashTransform.gameObject.SetActive(false);
            Debug.Log("Ataci cu pumnul! Nu ai sabia echipată pe primul slot.");
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);

        // Preluăm damage-ul, tot sub protecție
        int damageArma = 0;
        if (EquipmentManager.instance != null)
        {
            damageArma = EquipmentManager.instance.GetDamageCurent();
        }

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
            // Vameșul caută ambele legitimații:
            EnemyStats normalEnemy = enemy.GetComponent<EnemyStats>();
            GiantSlimeBoss boss = enemy.GetComponent<GiantSlimeBoss>();

            // Dacă a găsit MĂCAR unul dintre ei...
            if (normalEnemy != null || boss != null)
            {
                Vector2 directionToEnemy = (enemy.transform.position - transform.position).normalized;
                float angle = Vector2.Angle(movement.lastMoveDir, directionToEnemy);

                if (angle < attackAngle / 2f)
                {
                    // Cazul 1: E inamic mic
                    if (normalEnemy != null)
                    {
                        normalEnemy.TakeDamage(finalDamage);
                        normalEnemy.ApplyHitStun(0.3f);
                    }
                    // Cazul 2: E Tăticul Slime-urilor
                    else if (boss != null)
                    {
                        boss.TakeDamage(finalDamage);
                        // Opțional: îi dăm și lui un micro-stun, sau îl lăsăm tanc
                    }

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