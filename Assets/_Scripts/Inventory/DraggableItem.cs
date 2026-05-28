using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Interfețele astea 3 îi spun Unity-ului să asculte mouse-ul pentru acest obiect
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform parentAfterDrag; // Ține minte unde trebuie să se întoarcă
    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Avem nevoie de asta ca să facem iconița să nu blocheze mouse-ul când o tragem
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Am început să trag de un obiect!");
        parentAfterDrag = transform.parent; // Salvăm slotul din care a plecat

        // Îl scoatem din slot și îl punem direct pe Canvas-ul mare ca să se vadă peste toate celelalte UI-uri
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        // Facem iconița transparentă la click, ca mouse-ul să poată "vedea" slotul de dedesubt
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Iconița urmărește mouse-ul
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Am lăsat obiectul din mână.");
        transform.SetParent(parentAfterDrag);

        // NOU: Asta îl forțează să se centreze perfect în noua căsuță (nu îl lasă să zboare)
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        canvasGroup.blocksRaycasts = true;
    }
}