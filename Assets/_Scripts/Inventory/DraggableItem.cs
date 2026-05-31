using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

// NOU: Am creat categoriile de iteme posibile
public enum TipItem { Potiune, Arma, Armura, Material }

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Detalii Generale")]
    public string numeItem = "Potiune";
    public TipItem categoriaItemului = TipItem.Potiune; // Din Inspector alegi ce este!
    public TMP_Text textCantitateSauUpgrade; // Același text vizual, dar îi schimbăm rolul

    [Header("Pentru Poțiuni (Stacabile)")]
    public int cantitate = 1;

    [Header("Pentru Arme / Armuri")]
    public int nivelUpgrade = 0; // Ex: 0 înseamnă simplu, 1 înseamnă +1
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
        // Pentru fiecare nivel de upgrade, primești 5 damage în plus (poți schimba formula)
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
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject obiectPrimit = eventData.pointerDrag;
        DraggableItem itemPrimit = obiectPrimit.GetComponent<DraggableItem>();

        if (itemPrimit != null && itemPrimit != this)
        {
            // Doar poțiunile și materialele se pot stacha! Armele NU se stachează!
            if (this.numeItem == itemPrimit.numeItem && (this.categoriaItemului == TipItem.Potiune || this.categoriaItemului == TipItem.Material))
            {
                this.AdaugaCantitate(itemPrimit.cantitate);
                Destroy(obiectPrimit);
            }
        }
    }
}