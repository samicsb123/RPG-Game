using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Setări Atac")]
    public KeyCode attackKey = KeyCode.Space;
    public float attackRange = 1.0f;
    public int damage = 25;

    // NOU: Distanța față de jucător. O poți schimba din Inspector!
    // Am pus 0.3 ca start, înainte era efectiv 0.8 fix.
    [Range(0f, 1f)]
    public float attackOffset = 0.3f;

    [Header("Referințe")]
    public Animator swordAnimator;
    public Transform swordSlashTransform;
    public PlayerMovement movement;

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

        // Pasul magic: Poziționăm doar sabia
        PositionSlash();

        // Pornim animația
        swordAnimator.SetTrigger("Attack");

        // Logică Damage
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(swordSlashTransform.position, attackRange);
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyStats stats = enemy.GetComponent<EnemyStats>();
            if (stats != null) stats.TakeDamage(damage);
        }
    }

    void PositionSlash()
    {
        Vector2 dir = movement.lastMoveDir;
        swordSlashTransform.localRotation = Quaternion.identity;

        // Logica de poziționare în cruce (+) - FOLOSIM attackOffset
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) // Orizontal
        {
            if (dir.x > 0)
            { // DREAPTA
                // Înlocuim 0.8f cu variabila noastră
                swordSlashTransform.localPosition = new Vector3(attackOffset, 0, 0);
                swordSlashTransform.localEulerAngles = new Vector3(0, 0, 0);
            }
            else
            { // STANGA
                swordSlashTransform.localPosition = new Vector3(-attackOffset, 0, 0);
                swordSlashTransform.localEulerAngles = new Vector3(0, 0, 180f);
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
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (swordSlashTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(swordSlashTransform.position, attackRange);
        }
    }
}