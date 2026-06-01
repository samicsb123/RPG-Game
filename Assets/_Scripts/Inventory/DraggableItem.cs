using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

// NOU: Am creat categoriile de iteme posibile
public enum TipItem { Potiune, Arma, Armura, Material }

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
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
        TooltipManager.instance.AscundeTooltip();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 1. Sabia se întoarce vizual la locul ei în inventar
        transform.SetParent(parentAfterDrag);
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        canvasGroup.blocksRaycasts = true;

        // 2. Scanăm lumea 2D sub mouse folosind un radar complet (RaycastAll)
        if (categoriaItemului == TipItem.Arma || categoriaItemului == TipItem.Armura)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Luăm o listă cu TOATE obiectele care se află sub mouse în acea secundă
            RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, Vector2.zero);

            bool gasitFierar = false;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null)
                {
                    // Căutăm scriptul Fierarului pe oricare dintre obiectele lovite
                    BlacksmithDropZone fierar = hit.collider.GetComponent<BlacksmithDropZone>();

                    if (fierar != null)
                    {
                        Debug.Log("🎯 SUCCES: Am găsit Fierarul sub mouse-ul tău!");
                        fierar.PrimesteArma(this);
                        gasitFierar = true;
                        break; // Am găsit ce ne doream, oprim căutarea
                    }
                }
            }

            if (!gasitFierar)
            {
                Debug.LogWarning("❌ Radarul a scanat zona, dar nu a detectat niciun collider cu scriptul BlacksmithDropZone.");
            }
        }
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
    // Când intră mouse-ul pe item
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
            info += "\nCantitate: " + cantitate;
        }

        TooltipManager.instance.ArataTooltip(info);
    }

    // Când iese mouse-ul de pe item
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.instance.AscundeTooltip();
    }
}