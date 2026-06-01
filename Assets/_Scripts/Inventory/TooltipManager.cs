using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager instance; // Ca să îl putem apela de oriunde
    public TMP_Text tooltipText; // Trage textul din interiorul panoului aici

    void Awake()
    {
        instance = this;
        gameObject.SetActive(false); // Îl ascundem la începutul jocului
    }

    void Update()
    {
        // -15 pe X înseamnă spre stânga, +15 pe Y înseamnă în sus
        Vector3 offset = new Vector3(-15f, 15f, 0f);

        // Aplicăm distanța față de poziția mouse-ului
        transform.position = Input.mousePosition + offset;
    }

    public void ArataTooltip(string mesaj)
    {
        tooltipText.text = mesaj;
        gameObject.SetActive(true);
        // Ne asigurăm că tooltip-ul apare peste toate celelalte UI-uri
        transform.SetAsLastSibling();
    }

    public void AscundeTooltip()
    {
        gameObject.SetActive(false);
    }
}