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
        playerStats = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>(); // Luăm referința corpului fizic

        if (swordSlashTransform != null)
        {
            swordSlashTransform.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // 1. Verificăm dacă e mort (Prioritatea #1)
        if (playerStats != null && playerStats.isDead)
        {
            return;
        }

        // 2. NOU: Cronometrul pentru blocarea mișcării
        if (freezeTimer > 0)
        {
            freezeTimer -= Time.deltaTime;
            // Dacă timpul s-a scurs, îi dăm înapoi dreptul de a se mișca
            if (freezeTimer <= 0)
            {
                if (movement != null) movement.enabled = true;
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

        // --- NOU: Înghețăm jucătorul pe loc ---
        freezeTimer = attackFreezeDuration; // Setăm cronometrul la 0.5s
        movement.enabled = false;           // Oprim citirea tastelor de mișcare (WASD/Săgeți)
        if (rb != null) rb.velocity = Vector2.zero; // Îl oprim din alunecare (dacă era deja în mișcare)
        // --------------------------------------

        bool areArma = EquipmentManager.instance.AreArmaEchipata();

        if (areArma)
        {
            swordSlashTransform.gameObject.SetActive(true);
            PositionSlash();
            swordAnimator.SetTrigger("Attack");
            AudioManager.instance.PlaySound("Slash");
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

    // ... restul codului rămâne exact la fel (PositionSlash și OnDrawGizmosSelected) ...

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