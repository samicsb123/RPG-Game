using UnityEngine;

public class ToolbarManager : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public Transform[] sloturiToolbar;

    void Update()
    {
        // Ascultăm tastele de deasupra literelor (Alpha1 -> Alpha7)
        if (Input.GetKeyDown(KeyCode.Alpha1)) UseSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UseSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UseSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) UseSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) UseSlot(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) UseSlot(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) UseSlot(6);
    }

    void UseSlot(int indexSlot)
    {
        if (indexSlot >= sloturiToolbar.Length) return;

        Transform slot = sloturiToolbar[indexSlot];

        // NOU: Nu mai numărăm copiii, căutăm direct poțiunea
        DraggableItem item = slot.GetComponentInChildren<DraggableItem>();

        if (item != null && item.numeItem == "Potiune")
        {
            ConsumePotion(item);
        }
    }

    void ConsumePotion(DraggableItem potiune)
    {
        // 1. Dacă viața e deja plină, nu risipim poțiunea degeaba
        if (playerStats.currentHealth >= playerStats.maxHealth)
        {
            Debug.Log("Health is already full! Cannot consume potion.");
            return;
        }

        // 2. Dăm 30 Viață
        playerStats.currentHealth += 30;

        // 3. Dacă depășește maximul, tăiem surplusul
        if (playerStats.currentHealth > playerStats.maxHealth)
        {
            playerStats.currentHealth = playerStats.maxHealth;
        }

        // 4. Actualizăm bara de viață de pe ecran și textul
        if (playerStats.healthBar != null) playerStats.healthBar.SetHealth(playerStats.currentHealth);
        playerStats.UpdateHPText();
        Debug.Log("Consumed a potion! Health restored.");

        // 5. Scădem o bucată din stack-ul de poțiuni
        potiune.AdaugaCantitate(-1);

        // 6. Dacă am băut-o pe ultima (0 bucăți), distrugem sticla din inventar
        if (potiune.cantitate <= 0)
        {
            Destroy(potiune.gameObject);
        }
    }
}