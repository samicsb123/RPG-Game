using UnityEngine;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    [Header("Referințe UI")]
    public GameObject upgradePanel;
    public TMP_Text infoText;
    public TMP_Text chanceText;
    public PlayerStats playerStats; // Pentru a-i lua Gold-ul

    [Header("Setări Upgrade (Nivel 0 -> 8)")]
    // Index 0 e pentru upgrade-ul de la +0 la +1. Index 8 este pentru +8 la +9.
    public int[] costuriGold = { 50, 100, 150, 250, 400, 600, 800, 1200, 2000 };
    public int[] sanseReusita = { 100, 95, 90, 80, 70, 60, 50, 45, 40 };

    private DraggableItem itemDeUpgradat;

    public void DeschidePanou(DraggableItem item)
    {
        if (item.nivelUpgrade >= 9)
        {
            Debug.Log("Fierar: Arma a atins deja puterea maximă (+9)!");
            return;
        }

        itemDeUpgradat = item;
        upgradePanel.SetActive(true);
        ActualizeazaTexte();
    }

    void ActualizeazaTexte()
    {
        int nivel = itemDeUpgradat.nivelUpgrade;
        infoText.text = $"Upgrade {itemDeUpgradat.numeItem} (+{nivel} -> +{nivel + 1})\nCost: {costuriGold[nivel]} Gold";
        chanceText.text = $"Success rate: {sanseReusita[nivel]}%";

        // Culoarea textului de șansă în funcție de cât de riscant e
        if (sanseReusita[nivel] >= 80) chanceText.color = Color.green;
        else if (sanseReusita[nivel] >= 50) chanceText.color = Color.yellow;
        else chanceText.color = Color.red;
    }

    public void ConfirmaUpgrade()
    {
        if (itemDeUpgradat == null) return;

        int nivel = itemDeUpgradat.nivelUpgrade;
        int cost = costuriGold[nivel];
        int sansa = sanseReusita[nivel];

        // 1. Verificăm dacă avem bani
        if (playerStats.gold < cost)
        {
            Debug.Log("Fierar: Nu ai destul Gold pentru asta!");
            return;
        }

        // 2. Luăm banii jucătorului
        playerStats.gold -= cost;
        playerStats.UpdateUI();

        // 3. Dăm cu zarul pentru noroc (între 1 și 100)
        int zar = Random.Range(1, 101);

        if (zar <= sansa)
        {
            // SUCCES!
            itemDeUpgradat.nivelUpgrade++;
            itemDeUpgradat.ActualizeazaText(); // Să scrie pe ecran noul plus (+1, +2 etc)
            Debug.Log($"SUCCES! Ai acum {itemDeUpgradat.numeItem} +{itemDeUpgradat.nivelUpgrade}");

            if (itemDeUpgradat.nivelUpgrade >= 9) InchidePanou();
            else ActualizeazaTexte(); // Actualizăm pentru următorul nivel dacă vrea să continue
        }
        // ... codul de dinainte (cu succes sau eșec) ...
        else
        {
            // EȘEC!
            if (itemDeUpgradat.nivelUpgrade > 0)
            {
                itemDeUpgradat.nivelUpgrade--;
                Debug.Log($"Fierar: Upgrade EȘUAT! Sabia a retrogradat la +{itemDeUpgradat.nivelUpgrade}.");
            }
            else
            {
                Debug.Log("Fierar: Upgrade EȘUAT! Sabia a rămas la +0.");
            }

            itemDeUpgradat.ActualizeazaText();
            ActualizeazaTexte();
        }

        // NOU: SALVARE INSTANTANEE ANTI-CHEAT
        // Imediat ce zarul a fost dat și sabia modificată, salvăm jocul!
        FindObjectOfType<SaveManager>().SaveGame();
    } // <- Aici e finalul funcției ConfirmaUpgrade()

    public void InchidePanou()
    {
        upgradePanel.SetActive(false);
        itemDeUpgradat = null;
    }
}