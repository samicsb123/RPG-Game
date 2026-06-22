using UnityEngine;

public class BossHitbox : MonoBehaviour
{
    [Header("Setări Damage")]
    public int contactDamage = 65; // Exact ce ai cerut
    public float damageCooldown = 1f; // Cât timp ești imun după ce te lovește (1 secundă)

    private float lastDamageTime;

    // Se declanșează fix în secunda în care dă cu capul de tine
    private void OnCollisionEnter2D(Collision2D collision)
    {
        DealDamage(collision.gameObject);
    }

    // Se declanșează continuu dacă rămâi blocat lipit de el
    private void OnCollisionStay2D(Collision2D collision)
    {
        DealDamage(collision.gameObject);
    }

    private void DealDamage(GameObject target)
    {
        // Ne asigurăm că am lovit Jucătorul și că a trecut cooldown-ul de 1 secundă
        if (target.CompareTag("Player") && Time.time >= lastDamageTime + damageCooldown)
        {
            lastDamageTime = Time.time;

            // AICI TE LEGI DE SCRIPTUL TĂU DE VIAȚĂ AL JUCĂTORULUI!
            // Exemplu:
            PlayerStats playerStats = target.GetComponent<PlayerStats>();
            if (playerStats != null) playerStats.TakeDamage(contactDamage);

            Debug.Log($"CRITIC! Slime-ul te-a strivit și ți-a luat {contactDamage} HP!");
        }
    }
}