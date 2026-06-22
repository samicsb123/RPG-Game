using UnityEngine;

public class ToxicBullet : MonoBehaviour
{
    [Header("Setări Glonț")]
    public int bulletDamage = 25;
    public float lifeTime = 4f;

    void Start()
    {
        // Programăm autodistrugerea în caz că ratează tot
        Destroy(gameObject, lifeTime);
    }

    // Am schimbat în OnCOLLISION (impact fizic)
    void OnCollisionEnter2D(Collision2D collision)
    {
        // ALARMA 1: Ne spune de ce anume s-a izbit glonțul (chiar dacă e un perete sau podea)
        Debug.Log("Glonțul s-a izbit fizic de: " + collision.gameObject.name);

        // Verificăm dacă e Jucătorul
        if (collision.gameObject.CompareTag("Player"))
        {
            // ALARMA 2: Confirmă că Tag-ul a funcționat
            Debug.Log("Tag recunoscut: Este Player!");

            // Căutăm scriptul tău, inclusiv pe obiectul părinte (în caz că are un hitbox separat)
            PlayerStats playerStats = collision.gameObject.GetComponentInParent<PlayerStats>();

            if (playerStats != null)
            {
                // Lovitură directă!
                playerStats.TakeDamage(bulletDamage);
                Debug.Log($"BUM! S-a luat {bulletDamage} damage!");

                Destroy(gameObject); // Distruge glonțul
            }
            else
            {
                Debug.LogError("Atenție: A lovit Player-ul, dar motorul NU GĂSEȘTE scriptul PlayerStats pe el!");
            }
        }
        else if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject); // Se sparge de pereți
        }
    }
}