using UnityEngine;

public class BossArenaManager : MonoBehaviour
{
    [Header("Referințe Arenă")]
    public GiantSlimeBoss bossScript;
    public GameObject arenaDoor; // Zidul invizibil

    [Header("Audio")]
    public string bossMusicName = "BossTheme"; // Numele melodiei din AudioManager
    public string townMusicName = "TownTheme"; // Muzica de explorare/sat

    private bool arenaIsActive = false;

    void Start()
    {
        // Pornim muzica de meniu imediat ce intrăm în această scenă
        AudioManager.instance.PlayMusic("MainMenu");
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Dacă jucătorul a intrat în senzor și arena nu e deja pornită
        if (collision.CompareTag("Player") && !arenaIsActive)
        {
            StartBossFight();
        }
    }

    void StartBossFight()
    {
        arenaIsActive = true;

        // 1. Blochează ușa în spatele tău (activează obiectul)
        if (arenaDoor != null) arenaDoor.SetActive(true);

        // 2. Trezește Boss-ul
        if (bossScript != null) bossScript.WakeUpBoss();

        // 3. Schimbă muzica (Dacă ai AudioManager)
        if (AudioManager.instance != null) AudioManager.instance.PlaySound(bossMusicName);

        Debug.Log("ARENA ACTIVATĂ! Fight!");
    }

    // Această funcție va fi apelată când mori și dai "Respawn"
    public void ResetArena()
    {
        arenaIsActive = false;

        // 1. Deschide ușa înapoi
        if (arenaDoor != null) arenaDoor.SetActive(false);

        // 2. Resetează Boss-ul (viață full, îl adoarme la loc)
        if (bossScript != null) bossScript.ResetToSleep();

        // 3. Pune muzica liniștită la loc
        if (AudioManager.instance != null) AudioManager.instance.PlaySound(townMusicName);
    }
}