using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    // Facem scriptul "Static" ca să poată fi citit instantaneu de orice alt script din joc
    public static EquipmentManager instance;

    [Header("Referințe Inventar")]
    public InventorySlot slotArma; // Aici vei trage primul slot gri (VIP)

    [Header("Statistici Bază")]
    public int damagePumn = 2; // Damage-ul dacă ataci fără sabie echipată

    void Awake()
    {
        instance = this;
    }

    // Funcție care ne spune cu Adevărat/Fals dacă sabia e în slotul corect
    public bool AreArmaEchipata()
    {
        DraggableItem arma = slotArma.GetComponentInChildren<DraggableItem>();
        return arma != null && arma.categoriaItemului == TipItem.Arma;
    }

    // Funcție care calculează damage-ul total în momentul atacului
    public int GetDamageCurent()
    {
        DraggableItem arma = slotArma.GetComponentInChildren<DraggableItem>();

        if (arma != null && arma.categoriaItemului == TipItem.Arma)
        {
            return arma.GetDamageTotal(); // Returnează damage-ul bazei + upgrade-ul Fierarului
        }

        return damagePumn; // Dacă slotul e gol sau are altceva, dă damage minim
    }
}