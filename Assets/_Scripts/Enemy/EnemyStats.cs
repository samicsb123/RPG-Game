using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Statistici Inamic")]
    public int maxHealth = 50;
    public int currentHealth;
    public int attackDamage = 10;

    [Header("Setari Atac")]
    public float attackCooldown = 1.5f; // Puțin mai rar ca să nu moară playerul instant
    private float lastAttackTime = -100f;

    [Header("Recompense (Metin2 Style)")]
    public int xpReward = 25;
    public int goldMin = 10; // Mai bine punem un range clar
    public int goldMax = 20;

    void Start()
    {
        currentHealth = maxHealth;
    }

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
                    // Debug-ul tău e bun, îl lăsăm
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        // Aici poți adăuga un efect de sânge sau sclipire mai târziu

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        PlayerStats player = FindObjectOfType<PlayerStats>();
        if (player != null)
        {
            player.AddXP(xpReward);

            // Folosim range-ul de aur setat pentru acest inamic specific
            int randomGold = Random.Range(goldMin, goldMax + 1);
            player.AddGold(randomGold);

            Debug.Log($"Inamic învins! Ai primit {xpReward} XP și {randomGold} Gold.");
        }

        Destroy(gameObject); // Asta va declanșa automat Spawner-ul nostru!
    }
}