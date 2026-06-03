using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager instance;

    [Header("Referințe Inventar")]
    public InventorySlot slotArma;
    public GameObject placeholderArma; // NOU: Referința către imaginea gri

    [Header("Statistici Bază")]
    public int damagePumn = 2;

    void Awake()
    {
        instance = this;
    }

    // NOU: Jocul va verifica încontinuu dacă slotul este ocupat
    void Update()
    {
        if (placeholderArma != null)
        {
            // Dacă funcția AreArmaEchipata() este Falsă, placeholder-ul e vizibil (True).
            // Dacă funcția AreArmaEchipata() este Adevărată, placeholder-ul se ascunde (False).
            placeholderArma.SetActive(!AreArmaEchipata());
        }
    }

    public bool AreArmaEchipata()
    {
        DraggableItem arma = slotArma.GetComponentInChildren<DraggableItem>();
        // Atenție: Dacă ai eroare aici, e posibil să nu ai "TipItem.Arma" definit. 
        // Dacă îți dă eroare, folosește doar "return arma != null;"
        return arma != null && arma.categoriaItemului == TipItem.Arma;
    }

    public int GetDamageCurent()
    {
        DraggableItem arma = slotArma.GetComponentInChildren<DraggableItem>();

        if (arma != null && arma.categoriaItemului == TipItem.Arma)
        {
            return arma.GetDamageTotal();
        }

        return damagePumn;
    }
}