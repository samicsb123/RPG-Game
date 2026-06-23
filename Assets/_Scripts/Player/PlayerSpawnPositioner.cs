using UnityEngine;

public class PlayerSpawnPositioner : MonoBehaviour
{
    void Start()
    {
        // Verificăm dacă portalul din scena anterioară ne-a lăsat un bilet cu o adresă
        if (!string.IsNullOrEmpty(Portal.nextSpawnPointName))
        {
            // Căutăm în noua scenă obiectul care are FIX acel nume
            GameObject spawnPoint = GameObject.Find(Portal.nextSpawnPointName);
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (spawnPoint != null && player != null)
            {
                // Mutăm jucătorul pe poziția spawn point-ului
                player.transform.position = spawnPoint.transform.position;

                // Resetăm fizica ca să nu fie aruncat din inerție
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.velocity = Vector2.zero;

                Debug.Log($"Succes! Jucătorul a fost așezat la spawn-ul: {Portal.nextSpawnPointName}");
            }
            else
            {
                Debug.LogWarning($"Nu s-a putut spawna! SpawnPoint gasit: {spawnPoint != null}, Player gasit: {player != null}");
            }
        }
    }
}