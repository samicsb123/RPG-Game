using UnityEngine;
using TMPro; // Asigură-te că ai asta pentru texte

public class PlayerStats : MonoBehaviour
{
    [Header("Statistici Vitale")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Progresie (RPG)")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [Header("Economie")]
    public int gold = 0;

    [Header("UI Referințe")]
    public HealthBar healthBar;
    public TMP_Text xpText;   // Trage XPText aici în Inspector
    public TMP_Text goldText; // Trage GoldText aici în Inspector

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null) healthBar.SetMaxHealth(maxHealth);

        UpdateUI(); // Afișăm valorile corecte la start
    }

    // Funcția care adaugă XP (chemată de urs când moare)
    public void AddXP(int amount)
    {
        currentXP += amount;

        // Verificăm dacă am făcut nivel
        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }

        UpdateUI();
    }
    public void AddGold(int amount)
    {
        gold += amount;
        UpdateUI(); // Aceasta este linia care "repară" afișarea pe ecran
        Debug.Log("Ai primit " + amount + " Gold!");
    }
    void LevelUp()
    {
        level++;
        // Păstrăm XP-ul rămas (ex: dacă ai 110/100, rămâi cu 10 la lvl următor)
        currentXP -= xpToNextLevel;

        // Mărim dificultatea pentru nivelul următor (ex: cu 50% mai mult)
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f);

        Debug.Log("Felicitări! Ai ajuns la nivelul " + level);

        // Opțional: Îți dăm viața plină la level up
        currentHealth = maxHealth;
        if (healthBar != null) healthBar.SetHealth(currentHealth);
    }

    // Această funcție scrie pe ecran valorile exact cum ai cerut
    public void UpdateUI()
    {
        if (xpText != null)
        {
            // Format: XP: 10 / 100
            xpText.text = "XP: " + currentXP + " / " + xpToNextLevel + " (Lvl " + level + ")";
        }

        if (goldText != null)
        {
            // Format: Gold: 50
            goldText.text = "Gold: " + gold;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (healthBar != null) healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Jucătorul a murit!");
            // Aici poți pune logica de Game Over
        }
    }
}