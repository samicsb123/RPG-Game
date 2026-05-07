using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Statistici Inamic")]
    public int maxHealth = 50;
    public int currentHealth;
    public int attackDamage = 10;

    [Header("Setari Atac")]
    public float attackCooldown = 1.0f;
    private float lastAttackTime = -100f; // Seta de start mica pentru atac instant la prima atingere

    [Header("Recompense (Metin2 Style)")]
    public int xpReward = 25;
    public int goldReward = 15;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // Folosim Stay2D pentru damage continuu
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                PlayerStats player = collision.gameObject.GetComponent<PlayerStats>();
                if (player != null)
                {
                    player.TakeDamage(attackDamage);
                    lastAttackTime = Time.time;
                    Debug.Log("Ursul te mușcă! HP rămas: " + player.currentHealth);
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Ursul a luat damage. Viata ramasa: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Inamic invins!");

        PlayerStats player = FindObjectOfType<PlayerStats>();
        if (player != null)
        {
            // Adăugăm XP-ul fix setat în Inspector
            player.AddXP(xpReward);
            int randomGold = Random.Range(100, 151);

            // Trimitem aurul către jucător
            player.AddGold(randomGold);
        }

        Destroy(gameObject); // Ursul dispare
    }
}