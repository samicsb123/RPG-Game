using UnityEngine;
using System.Collections;

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

    [Header("UI Bara de Viata")]
    public HealthBar healthBar; // NOU: Referința către scriptul de pe Slider

    void Start()
    {
        currentHealth = maxHealth;

        // NOU: Când se spawnează ursul, setăm slider-ul la maxHealth
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // --- REPARAȚIA SUPREMĂ PENTRU PREFABURI ---
        // Căutăm PlayerStats pe obiectul atins SAU pe părinții lui din ierarhie.
        // Asta garantează că Ursul detectează Jucătorul, chiar dacă îi lovește doar „sabia” sau un collider secundar.
        PlayerStats player = collision.gameObject.GetComponentInParent<PlayerStats>();

        if (player != null && !player.isDead)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                player.TakeDamage(attackDamage);
                lastAttackTime = Time.time;
                Debug.Log("Ursul a atacat cu succes Jucătorul și i-a provocat damage!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // NOU: Când își ia damage, actualizăm bara să scadă
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // NOU: Funcția pornește cronometrul de înghețare pentru inamic
    public void ApplyHitStun(float duration)
    {
        StartCoroutine(HitStunCoroutine(duration));
    }

    private IEnumerator HitStunCoroutine(float duration)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // 1. Înghețăm inamicul complet pe loc (oprim fizica și viteza)
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false; // Îngheață poziția și fizica complet
        }

        // Așteptăm cele 0.3 secunde cerute
        yield return new WaitForSeconds(duration);

        // 2. Îi dăm drumul înapoi inamicului să se miște și să te atace
        if (rb != null)
        {
            rb.simulated = true;
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

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySound("DeadBear");
        }
    }
}