using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Statistici Vitale")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI Referințe")]
    public HealthBar healthBar; // Legătura cu scriptul HealthBar creat din tutorial

    [Header("Progresie (RPG)")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [Header("Economie")]
    public int gold = 0;

    void Start()
    {
        // La începutul jocului, viața este la maxim
        currentHealth = maxHealth;

        // Anunțăm interfața (UI) care este viața maximă
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }
    }

    void Update()
    {
        // TEST: Când apeși tasta SPACE, jucătorul ia 20 damage
        // (Asta e doar ca să testezi tu bara de viață până punem inamici reali)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(20);
        }
    }

    // Funcția care scade viața
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Nu lăsăm viața să scadă sub 0
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        // Actualizăm slider-ul de pe ecran
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        // Verificăm dacă jucătorul a murit
        if (currentHealth == 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Jucătorul a murit!");
        // Aici vom declanșa animația de moarte mai târziu
    }

    // Funcție pentru când omori inamici
    public void AddXP(int amount)
    {
        currentXP += amount;
        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        currentXP -= xpToNextLevel;

        // Scalare dificultate: următorul nivel cere cu 50% mai mult XP
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f);

        // La level up, crește viața maximă și o vindecă complet
        maxHealth += 20;
        currentHealth = maxHealth;

        // Actualizăm UI-ul cu noile limite
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }
    }
}