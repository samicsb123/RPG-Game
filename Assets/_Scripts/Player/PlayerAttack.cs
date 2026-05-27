using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Setări Atac")]
    public KeyCode attackKey = KeyCode.Space;
    public float attackRange = 1.0f;

    // NOU: Unghiul conului (deschiderea "feliei de pizza"). 120 e perfect pentru săbii.
    [Range(0f, 360f)]
    public float attackAngle = 120f;

    [Range(0f, 1f)]
    public float attackOffset = 0.3f;

    [Header("Referințe")]
    public Animator swordAnimator;
    public Transform swordSlashTransform;
    public PlayerMovement movement;

    // NOU: Facem legătura cu statusurile tale pentru a lua damage-ul real!
    private PlayerStats playerStats;

    void Start()
    {
        // Găsim automat scriptul de statusuri când începe jocul
        playerStats = GetComponent<PlayerStats>();
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

        // Poziționăm animația sabiei
        PositionSlash();
        swordAnimator.SetTrigger("Attack");

        // 1. Căutăm inamicii din jurul nostru (luăm centrul jucătorului ca punct de plecare)
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyStats stats = enemy.GetComponent<EnemyStats>();
            if (stats != null)
            {
                // 2. Calculăm direcția de la tine spre inamic
                Vector2 directionToEnemy = (enemy.transform.position - transform.position).normalized;

                // 3. Calculăm unghiul dintre direcția în care te uiți (lastMoveDir) și inamic
                float angle = Vector2.Angle(movement.lastMoveDir, directionToEnemy);

                // 4. Lovim DOAR dacă unghiul este în interiorul conului nostru
                if (angle < attackAngle / 2f)
                {
                    // Luăm damage-ul real din PlayerStats. Dacă nu-l găsește din ceva motiv, dă 25.
                    int actualDamage = (playerStats != null) ? playerStats.damage : 25;

                    stats.TakeDamage(actualDamage);
                }
            }
        }
    }

    void PositionSlash()
    {
        Vector2 dir = movement.lastMoveDir;
        swordSlashTransform.localRotation = Quaternion.identity;

        // Resetăm scara la normal (1, 1, 1) înainte de fiecare atac
        swordSlashTransform.localScale = new Vector3(1, 1, 1);

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) // Orizontal
        {
            if (dir.x > 0)
            { // DREAPTA
                swordSlashTransform.localPosition = new Vector3(attackOffset, 0, 0);
                swordSlashTransform.localEulerAngles = new Vector3(0, 0, 0);
            }
            else
            { // STANGA
                swordSlashTransform.localPosition = new Vector3(-attackOffset, 0, 0);
                swordSlashTransform.localEulerAngles = new Vector3(0, 0, 180f);
                // TRUCUL MAGIC: Răsturnăm axa Y ca animația să nu fie cu capul în jos
                swordSlashTransform.localScale = new Vector3(1, -1, 1);
            }
        }
        else // Vertical
        {
            if (dir.y > 0)
            { // SUS
                swordSlashTransform.localPosition = new Vector3(0, attackOffset, 0);
                swordSlashTransform.localEulerAngles = new Vector3(0, 0, 90f);
            }
            else
            { // JOS
                swordSlashTransform.localPosition = new Vector3(0, -attackOffset, 0);
                swordSlashTransform.localEulerAngles = new Vector3(0, 0, -90f);
                // TRUCUL MAGIC: Răsturnăm axa Y și aici pentru a păstra direcția tăieturii
                swordSlashTransform.localScale = new Vector3(1, -1, 1);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // Am mutat vizualizarea pe centrul jucătorului ca să vezi exact raza din care pornește conul
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}