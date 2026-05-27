using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Statistici Personaj")]
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
    public int gold = 0;

    [Header("UI Referințe Principale")]
    public HealthBar healthBar;
    public TMP_Text hpText;
    public TMP_Text xpText;
    public TMP_Text goldText;

    [Header("Game Over & Respawn (FĂRĂ PANOU)")]
    public Button townButton;
    public Button spotButton;
    public TMP_Text townButtonText;
    public TMP_Text spotButtonText;
    public Transform townSpawnPoint;
    public Animator anim;
    public bool isDead = false;

    private float townTimer;
    private float spotTimer;
    private bool townTimerActive = false;
    private bool spotTimerActive = false;

    [Header("UI Status Menu")]
    public GameObject statusMenuPanel;
    public TMP_Text pointsText;
    public TMP_Text strText;
    public TMP_Text vitText;

    void Start()
    {
        if (statusMenuPanel != null) statusMenuPanel.SetActive(false);

        // Ascundem butoanele de respawn la startul jocului
        if (townButton != null) townButton.gameObject.SetActive(false);
        if (spotButton != null) spotButton.gameObject.SetActive(false);

        if (anim == null) anim = GetComponent<Animator>();

        RecalculateStats();
        currentHealth = maxHealth;
        if (healthBar != null) healthBar.SetMaxHealth(maxHealth);

        UpdateUI();
        UpdateHPText();
    }

    void Update()
    {
        // Dacă e mort, rulăm doar cronometrele butoanelor
        if (isDead)
        {
            HandleRespawnTimers();
            return;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (statusMenuPanel != null)
            {
                bool isMenuActive = !statusMenuPanel.activeSelf;
                statusMenuPanel.SetActive(isMenuActive);
                if (isMenuActive) UpdateUI();
            }
        }
    }

    void HandleRespawnTimers()
    {
        // Cronometru pentru Respawn City (3 secunde)
        if (townTimerActive)
        {
            townTimer -= Time.deltaTime;
            if (townTimer <= 0)
            {
                townTimerActive = false;
                if (townButton != null) townButton.interactable = true;
                if (townButtonText != null) townButtonText.text = "Respawn city";
            }
            else if (townButtonText != null) townButtonText.text = $"Respawn city ({Mathf.CeilToInt(townTimer)}s)";
        }

        // Cronometru pentru Respawn Here (5 secunde)
        if (spotTimerActive)
        {
            spotTimer -= Time.deltaTime;
            if (spotTimer <= 0)
            {
                spotTimerActive = false;
                if (spotButton != null) spotButton.interactable = true;
                if (spotButtonText != null) spotButtonText.text = "Respawn here";
            }
            else if (spotButtonText != null) spotButtonText.text = $"Respawn here ({Mathf.CeilToInt(spotTimer)}s)";
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        if (healthBar != null) healthBar.SetHealth(currentHealth);
        UpdateHPText();

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        currentHealth = 0;
        if (hpText != null) hpText.text = "0 / " + maxHealth;

        // Oprim mișcarea jucătorului
        var movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        // Declanșăm animația de moarte (trebuie să ai un Trigger numit "Die" în Animator)
        if (anim != null) anim.SetTrigger("Die");

        // Afișăm butoanele pe ecran și le blocăm temporar interactiunea
        if (townButton != null) { townButton.gameObject.SetActive(true); townButton.interactable = false; }
        if (spotButton != null) { spotButton.gameObject.SetActive(true); spotButton.interactable = false; }

        // Setăm timpii de așteptare
        townTimer = 3f;
        spotTimer = 5f;
        townTimerActive = true;
        spotTimerActive = true;

        transform.rotation = Quaternion.Euler(0, 0, -90);
        GetComponent<SpriteRenderer>().color = Color.gray;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<Rigidbody2D>().simulated = false;
    }

    // Funcția chemată de butonul "Respawn city"
    public void RespawnInTown()
    {
        if (townSpawnPoint != null) transform.position = townSpawnPoint.position; // Teleportare
        currentHealth = maxHealth; // Viață plină
        ResetPlayerAfterRespawn();
    }

    // Funcția chemată de butonul "Respawn here"
    public void RespawnOnSpot()
    {
        currentHealth = Mathf.RoundToInt(maxHealth * 0.2f); // 20% din viață
        ResetPlayerAfterRespawn();
    }

    void ResetPlayerAfterRespawn()
    {
        isDead = false;

        if (healthBar != null) healthBar.SetHealth(currentHealth);
        UpdateHPText();

        // Pornim înapoi mișcarea
        var movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = true;

        // Ascundem butoanele înapoi
        if (townButton != null) townButton.gameObject.SetActive(false);
        if (spotButton != null) spotButton.gameObject.SetActive(false);

        // Resetăm animația
        if (anim != null)
        {
            anim.Play("Idle"); // Pune aici numele stării tale de bază din Animator dacă nu e "Idle" (ex: "Blend Tree")
            anim.ResetTrigger("Die");
        }
        transform.rotation = Quaternion.Euler(0, 0, 0);
        GetComponent<SpriteRenderer>().color = Color.white;
        GetComponent<Rigidbody2D>().simulated = true;
    }

    // Funcțiile vechi lăsate intacte
    public void RecalculateStats() { int oldMaxHealth = maxHealth; maxHealth = vitality * 10; damage = strength * 2; if (healthBar != null && maxHealth > oldMaxHealth) healthBar.SetMaxHealth(maxHealth); }
    public void AddXP(int amount) { if (isDead) return; currentXP += amount; while (currentXP >= xpToNextLevel) LevelUp(); UpdateUI(); }
    public void AddGold(int amount) { if (isDead) return; gold += amount; UpdateUI(); }
    void LevelUp() { level++; currentXP -= xpToNextLevel; xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f); statPointsAvailable += 3; currentHealth = maxHealth; if (healthBar != null) healthBar.SetHealth(currentHealth); UpdateUI(); UpdateHPText(); }
    public void UpgradeStrength() { if (statPointsAvailable > 0) { strength++; statPointsAvailable--; RecalculateStats(); UpdateUI(); } }
    public void UpgradeVitality() { if (statPointsAvailable > 0) { vitality++; statPointsAvailable--; RecalculateStats(); currentHealth += 10; if (healthBar != null) healthBar.SetHealth(currentHealth); UpdateUI(); UpdateHPText(); } }
    public void UpdateUI() { if (xpText != null) xpText.text = "XP: " + currentXP + " / " + xpToNextLevel + " (Lvl " + level + ")"; if (goldText != null) goldText.text = "Gold: " + gold; if (strText != null) { pointsText.text = "Puncte Disponibile: " + statPointsAvailable; strText.text = "Strength: " + strength; vitText.text = "Vitality: " + vitality; } }
    public void UpdateHPText() { if (hpText != null) hpText.text = currentHealth + " / " + maxHealth; }
}