using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Statistici Personaj (Puncte)")]
    public int strength = 5;
    public int dexterity = 5;
    public int vitality = 5;

    [Header("Atribute Vitale Calculate")]
    public int maxHealth;
    public int currentHealth;
    public int damage;

    [Header("Progresie (RPG)")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;
    public int statPointsAvailable = 0;

    [Header("Economie")]
    public int gold = 0;

    [Header("UI Referințe Principale")]
    public HealthBar healthBar;
    public TMP_Text hpText;      // <-- NOU: Textul de pe bara de HP
    public TMP_Text xpText;
    public TMP_Text goldText;

    [Header("UI Status Menu Referințe (Panel)")]
    public GameObject statusMenuPanel;
    public TMP_Text pointsText;
    public TMP_Text strText;
    public TMP_Text vitText;

    void Start()
    {
        if (statusMenuPanel != null) statusMenuPanel.SetActive(false);

        RecalculateStats();

        currentHealth = maxHealth;
        if (healthBar != null) healthBar.SetMaxHealth(maxHealth);

        UpdateUI();
        UpdateHPText(); // <-- NOU: Setăm textul de viață la start
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (statusMenuPanel != null)
            {
                bool isMenuActive = !statusMenuPanel.activeSelf;
                statusMenuPanel.SetActive(isMenuActive);

                if (isMenuActive)
                {
                    UpdateUI();
                }
            }
        }
    }

    public void RecalculateStats()
    {
        int oldMaxHealth = maxHealth;
        maxHealth = vitality * 10;
        damage = strength * 2;

        if (healthBar != null && maxHealth > oldMaxHealth)
        {
            healthBar.SetMaxHealth(maxHealth);
        }
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
        UpdateUI();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateUI();
    }

    void LevelUp()
    {
        level++;
        currentXP -= xpToNextLevel;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f);

        statPointsAvailable += 3;

        currentHealth = maxHealth;
        if (healthBar != null) healthBar.SetHealth(currentHealth);

        UpdateUI();
        UpdateHPText(); // <-- NOU
    }

    public void UpgradeStrength()
    {
        if (statPointsAvailable > 0)
        {
            strength++;
            statPointsAvailable--;
            RecalculateStats();
            UpdateUI();
        }
    }

    public void UpgradeVitality()
    {
        if (statPointsAvailable > 0)
        {
            vitality++;
            statPointsAvailable--;
            RecalculateStats();

            currentHealth += 10;
            if (healthBar != null) healthBar.SetHealth(currentHealth);

            UpdateUI();
            UpdateHPText(); // <-- NOU
        }
    }

    public void UpdateUI()
    {
        if (xpText != null) xpText.text = "XP: " + currentXP + " / " + xpToNextLevel + " (Lvl " + level + ")";
        if (goldText != null) goldText.text = "Gold: " + gold;

        if (strText != null)
        {
            pointsText.text = "Puncte Disponibile: " + statPointsAvailable;
            strText.text = "Strength: " + strength;
            vitText.text = "Vitality: " + vitality;
        }
    }

    // Funcția nouă care scrie x / y pe ecran
    public void UpdateHPText()
    {
        if (hpText != null)
        {
            hpText.text = currentHealth + " / " + maxHealth;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (healthBar != null) healthBar.SetHealth(currentHealth);

        UpdateHPText(); // <-- NOU: Când iei damage, scade și numărul

        if (currentHealth <= 0)
        {
            Debug.Log("Jucătorul a murit!");
            // Aici poți asigura că nu scrie cu minus (ex: 0 / 100)
            if (hpText != null) hpText.text = "0 / " + maxHealth;
        }
    }
}