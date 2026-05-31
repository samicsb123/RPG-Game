using UnityEngine;
using TMPro; // Avem nevoie de asta pentru text

public class ShopNPC : MonoBehaviour
{
    public enum ShopType { Fierar, Potiuni }

    [Header("Setãri Magazin")]
    public ShopType tipMagazin;
    public int pret = 20;

    [Header("Referin?e (Doar pentru Potiuni)")]
    public GameObject potionPrefab; // ?ablonul (Prefab) po?iunii
    public InventoryManager inventoryManager; // Scriptul care gestioneazã inventarul

    [Header("UI Magazin")]
    public TMP_Text textInteractiune; // Textul care se aprinde deasupra magazinului

    private bool jucatorInZona = false;
    private PlayerStats playerStats; // Ca sã luãm banii

    void Start()
    {
        // Ascundem textul la început ca sã nu stea mereu pe ecran
        if (textInteractiune != null)
        {
            textInteractiune.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Dacã e?ti în zonã ?i ape?i E
        if (jucatorInZona && Input.GetKeyDown(KeyCode.E))
        {
            Cumpara();
        }
    }

    void Cumpara()
    {
        if (playerStats == null) return;

        if (playerStats.gold >= pret)
        {
            if (tipMagazin == ShopType.Fierar)
            {
                playerStats.gold -= pret;
                playerStats.UpdateUI(); // <-- AICI: Actualizãm textul pe ecran

                // Aici vom adãuga codul pentru damage-ul sabiei mai târziu
                Debug.Log("Fierar: Ai upgradat arma!");
            }
            else if (tipMagazin == ShopType.Potiuni)
            {
                // Încercãm sã punem po?iunea în inventar
                bool adaugatCuSucces = inventoryManager.AdaugaItem(potionPrefab);

                if (adaugatCuSucces)
                {
                    playerStats.gold -= pret;
                    playerStats.UpdateUI(); // <-- AICI: Actualizãm textul pe ecran

                    Debug.Log("Magazin: Ai cumpãrat o po?iune!");
                }
                else
                {
                    Debug.Log("Magazin: Inventarul tãu este plin!");
                }
            }
        }
        else
        {
            Debug.Log("Magazin: Nu ai destul Gold!");
        }
    }

    // Func?iile care aprind/sting textul când intri în zonã (Toggle-ul)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jucatorInZona = true;
            playerStats = collision.GetComponent<PlayerStats>();

            if (textInteractiune != null) textInteractiune.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jucatorInZona = false;
            playerStats = null;

            if (textInteractiune != null) textInteractiune.gameObject.SetActive(false);
        }
    }
}