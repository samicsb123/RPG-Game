using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Trage aici MainInventory ?I Toolbar-ul")]
    public Transform[] toateInventarele;

    public bool AdaugaItem(GameObject prefabItem)
    {
        DraggableItem itemDeAdaugat = prefabItem.GetComponent<DraggableItem>();

        // 1. Cãutãm stack-uri existente
        foreach (Transform inventar in toateInventarele)
        {
            foreach (Transform slot in inventar)
            {
                // NOU: Cãutãm direct scriptul de item, ignorând textele
                DraggableItem itemDinSlot = slot.GetComponentInChildren<DraggableItem>();

                if (itemDinSlot != null && itemDinSlot.numeItem == itemDeAdaugat.numeItem)
                {
                    itemDinSlot.AdaugaCantitate(1);
                    return true;
                }
            }
        }

        // 2. Cãutãm sloturi goale
        foreach (Transform inventar in toateInventarele)
        {
            foreach (Transform slot in inventar)
            {
                // Dacã NU gãsim o po?iune în acest slot, înseamnã cã e liber
                if (slot.GetComponentInChildren<DraggableItem>() == null)
                {
                    Instantiate(prefabItem, slot);
                    return true;
                }
            }
        }

        return false;
    }
}