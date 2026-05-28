using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [Header("Referința către Inventar")]
    public GameObject mainInventoryPanel;

    // Această funcție va fi apelată de butonul tău
    public void ToggleVisibility()
    {
        if (mainInventoryPanel != null)
        {
            // activeSelf verifică dacă panoul este aprins sau stins în acest moment
            bool isCurrentlyOpen = mainInventoryPanel.activeSelf;

            // Setăm starea opusă (dacă e deschis, îl închide; dacă e închis, îl deschide)
            mainInventoryPanel.SetActive(!isCurrentlyOpen);
        }
    }
}