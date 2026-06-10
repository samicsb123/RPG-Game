using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

// NOU: Am creat categoriile de iteme posibile
public enum TipItem { Potiune, Arma, Armura, Material }

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("General Details")]
    public string numeItem = "Potion";
    public TipItem categoriaItemului = TipItem.Potiune; // Choose what it is from the Inspector!
    public TMP_Text textCantitateSauUpgrade; // The same visual text, but we change its role

    [Header("For Potions (Stackable)")]
    public int cantitate = 1;

    [Header("For Weapons / Armor")]
    public int nivelUpgrade = 0; // Ex: 0 means simple, 1 means +1
    public int damageDeBaza = 10;

    [HideInInspector] public Transform parentAfterDrag;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        ActualizeazaText();
    }

    // Funcție nouă: Calculează damage-ul cu tot cu upgrade!
    public int GetDamageTotal()
    {
        // For each upgrade level, you get 5 extra damage (you can change the formula)
        return damageDeBaza + (nivelUpgrade * 5);
    }

    public void AdaugaCantitate(int cat)
    {
        cantitate += cat;
        ActualizeazaText();
    }

    // NOU: Acum textul arată ori "3" (la poțiuni), ori "+1" (la arme)
    public void ActualizeazaText()
    {
        if (textCantitateSauUpgrade != null)
        {
            if (categoriaItemului == TipItem.Arma || categoriaItemului == TipItem.Armura)
            {
                textCantitateSauUpgrade.text = nivelUpgrade > 0 ? "+" + nivelUpgrade : "";
            }
            else
            {
                textCantitateSauUpgrade.text = cantitate > 1 ? cantitate.ToString() : "";
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
        TooltipManager.instance.AscundeTooltip();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 1. The weapon visually returns to its slot in the inventory
        transform.SetParent(parentAfterDrag);
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        canvasGroup.blocksRaycasts = true;

        // 2. Scan the 2D world under the mouse using a full radar (RaycastAll)
        if (categoriaItemului == TipItem.Arma || categoriaItemului == TipItem.Armura)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Get a list of ALL objects under the mouse at that exact frame
            RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, Vector2.zero);

            bool gasitFierar = false;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null)
                {
                    // Look for the Blacksmith script on any of the hit objects
                    BlacksmithDropZone fierar = hit.collider.GetComponent<BlacksmithDropZone>();

                    if (fierar != null)
                    {
                        Debug.Log("🎯 SUCCESS: Found the Blacksmith under your mouse!");
                        fierar.PrimesteArma(this);
                        gasitFierar = true;
                        break; // Found what we wanted, stop searching
                    }
                }
            }

            if (!gasitFierar)
            {
                Debug.LogWarning("❌ Radar scanned the area, but did not detect any collider with the BlacksmithDropZone script.");
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerEnter != null)
            Debug.Log("Mouse hit: " + eventData.pointerEnter.name);
        else
            Debug.Log("Mouse dropped in empty space!");

        GameObject obiectPrimit = eventData.pointerDrag;
        DraggableItem itemPrimit = obiectPrimit.GetComponent<DraggableItem>();

        if (itemPrimit != null && itemPrimit != this)
        {
            // Doar poțiile și materialele se pot stacha! Armele NU se stachează!
            if (this.numeItem == itemPrimit.numeItem && (this.categoriaItemului == TipItem.Potiune || this.categoriaItemului == TipItem.Material))
            {
                this.AdaugaCantitate(itemPrimit.cantitate);
                Destroy(obiectPrimit);
            }
        }
    }

    // When the mouse enters the item
    public void OnPointerEnter(PointerEventData eventData)
    {
        string info = numeItem;

        if (categoriaItemului == TipItem.Arma || categoriaItemului == TipItem.Armura)
        {
            info += " +" + nivelUpgrade;
            info += "\nDamage: " + GetDamageTotal();
        }
        else if (categoriaItemului == TipItem.Potiune)
        {
            info += "\nQuantity: " + cantitate;
        }

        TooltipManager.instance.ArataTooltip(info);
    }

    // When the mouse exits the item
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.instance.AscundeTooltip();
    }
}