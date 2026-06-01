using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Setări Critice")]
    [Range(0, 100)]
    public int sansaCritica = 20; // Ai 20% șansă să dai critică
    public float multiplicatorCritic = 2.0f; // Critica dă damage dublu (x2)

    [Header("Setări Atac")]
    public KeyCode attackKey = KeyCode.Space;
    public float attackRange = 1.0f;

    [Range(0f, 360f)]
    public float attackAngle = 120f;

    [Range(0f, 1f)]
    public float attackOffset = 0.3f;

    [Header("Referințe")]
    public Animator swordAnimator;
    public Transform swordSlashTransform;
    public PlayerMovement movement;

    // NOU: Aici declarăm referința pentru textul zburător
    public DamagePopup damagePopupPrefab;

    private PlayerStats playerStats;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();

        // La începutul jocului, ascundem animația de sabie până verificăm dacă are una
        if (swordSlashTransform != null)
        {
            swordSlashTransform.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(attackKey))
        {
            Attack();
        }
    }

    void Attack()
    {
        if (movement == null || swordAnimator == null || swordSlashTransform == null) return;

        // 1. ÎNTREBĂM MANAGERUL DE ECHIPAMENT DACĂ AVEM SABIA ECHIPATĂ PE SLOTUL 1
        bool areArma = EquipmentManager.instance.AreArmaEchipata();

        if (areArma)
        {
            // Dacă sabia e pe slotul 1, pornim vizualul și animația!
            swordSlashTransform.gameObject.SetActive(true);
            PositionSlash();
            swordAnimator.SetTrigger("Attack");
        }
        else
        {
            // Dacă nu are sabia pe slotul 1, dă cu pumnul. Ascundem efectul de tăietură.
            swordSlashTransform.gameObject.SetActive(false);
            Debug.Log("Ataci cu pumnul! Nu ai sabia echipată pe primul slot.");
        }

        // 2. Căutăm inamicii din jurul nostru
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);

        // 3. CALCULĂM DAMAGE-UL TOTAL (Armă + Strength)
        int damageArma = EquipmentManager.instance.GetDamageCurent();
        int bonusStrength = 0;

        if (playerStats != null)
        {
            // 1 punct de Strength = 2 Damage în plus
            bonusStrength = playerStats.strength * 2;
        }

        int actualDamage = damageArma + bonusStrength;

        // 4. CALCULĂM ȘANSA DE CRITICĂ (Șansa de bază + Dexteritate)
        int sansaTotalaCritica = sansaCritica;

        if (playerStats != null)
        {
            // Fiecare punct de Dexteritate adaugă 1% la șansă
            sansaTotalaCritica += playerStats.dexterity;
        }

        // Dăm cu zarul pentru critică o singură dată
        bool esteCritic = Random.Range(1, 101) <= sansaTotalaCritica;
        int finalDamage = actualDamage;

        if (esteCritic)
        {
            finalDamage = Mathf.RoundToInt(actualDamage * multiplicatorCritic);
        }

        // Variabilă ca să știm dacă am lovit măcar un inamic cu acest atac
        bool amLovitMacarUnInamic = false;

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyStats stats = enemy.GetComponent<EnemyStats>();

            if (stats != null)
            {
                Vector2 directionToEnemy = (enemy.transform.position - transform.position).normalized;
                float angle = Vector2.Angle(movement.lastMoveDir, directionToEnemy);

                // Lovim DOAR dacă unghiul este în interiorul conului nostru
                if (angle < attackAngle / 2f)
                {
                    // Aplicăm damage-ul fiecărui inamic atins
                    stats.TakeDamage(finalDamage);
                    amLovitMacarUnInamic = true;
                }
            }
        }

        // 5. Dacă am lovit cel puțin un inamic, generăm UN SINGUR text zburător
        if (amLovitMacarUnInamic && damagePopupPrefab != null)
        {
            // Îl generăm puțin mai sus de jucător (pe axa Y), ca să fie mereu vizibil
            Vector3 pozitieText = transform.position + new Vector3(0, 1f, 0);

            DamagePopup popup = Instantiate(damagePopupPrefab, pozitieText, Quaternion.identity);
            popup.SeteazaDamage(finalDamage, esteCritic);

            Debug.Log($"Lovitură reușită! Damage total: {finalDamage} (Critic: {esteCritic})");
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