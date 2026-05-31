using UnityEngine;
using UnityEngine.EventSystems;

// NOU: Ce rol are această căsuță?
public enum TipSlot { Normal, SlotArma, SlotArmura }

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public TipSlot tipulCăsuței = TipSlot.Normal; // Implicit, e un slot normal

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem itemInSlot = GetComponentInChildren<DraggableItem>();

        if (itemInSlot == null) // E gol?
        {
            GameObject obiectAruncat = eventData.pointerDrag;
            DraggableItem itemDeMutat = obiectAruncat.GetComponent<DraggableItem>();

            if (itemDeMutat != null)
            {
                // NOU: Filtrul de Securitate!
                if (tipulCăsuței == TipSlot.SlotArma && itemDeMutat.categoriaItemului != TipItem.Arma)
                {
                    Debug.Log("Aici poți pune doar arme!");
                    return; // Nu îl lăsăm
                }

                if (tipulCăsuței == TipSlot.SlotArmura && itemDeMutat.categoriaItemului != TipItem.Armura)
                {
                    Debug.Log("Aici poți pune doar armuri!");
                    return; // Nu îl lăsăm
                }

                // Dacă a trecut de securitate, îl lăsăm să intre
                itemDeMutat.parentAfterDrag = transform;
            }
        }
    }
}