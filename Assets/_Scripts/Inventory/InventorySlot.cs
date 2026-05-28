using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // Verificãm dacã slotul este GOAL (nu are copii)
        if (transform.childCount == 0)
        {
            // Luãm obiectul pe care tocmai l-am "scãpat" din mouse
            GameObject dropped = eventData.pointerDrag;

            DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
            if (draggableItem != null)
            {
                // Îi spunem obiectului cã noul lui pãrinte (casa lui) este acest slot!
                draggableItem.parentAfterDrag = transform;
            }
        }
    }
}