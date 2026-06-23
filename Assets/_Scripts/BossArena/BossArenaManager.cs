using UnityEngine;
using UnityEngine.Rendering.Universal; // Necesită pachetul URP instalat

public class BossArenaManager : MonoBehaviour
{
    [Header("Referințe Arenă")]
    public GiantSlimeBoss bossScript;
    public GameObject arenaDoor;

    [Header("Sistem Lumini (Vizibilitate)")]
    public Light2D globalLight;       // Trage obiectul Global Light 2D aici
    [Range(0f, 1f)] public float intunericArena = 0.1f;
    private float luminaNormala;      // Intensitatea de zi
    private Light2D playerLight;      // O găsim automat pe player

    [Header("Audio")]
    public string bossMusicName = "BossMusic";

    private bool arenaIsActive = false;

    void Start()
    {
        // Salvăm intensitatea luminii de zi
        if (globalLight != null) luminaNormala = globalLight.intensity;

        // Pornim muzica
        if (AudioManager.instance != null)
            AudioManager.instance.PlayMusic(bossMusicName);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !arenaIsActive)
        {
            StartBossFight();
        }
    }

    void StartBossFight()
    {
        arenaIsActive = true;

        // 1. Închide ușa
        if (arenaDoor != null) arenaDoor.SetActive(true);

        // 2. Trezește Boss-ul
        if (bossScript != null) bossScript.WakeUpBoss();

        // 3. Stinge lumina globală (creăm atmosfera de arenă)
        if (globalLight != null) globalLight.intensity = intunericArena;

        // 4. Aprinde lumina pe Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerLight = player.GetComponentInChildren<Light2D>(true);
            if (playerLight != null) playerLight.enabled = true;
        }

        Debug.Log("ARENA ACTIVATĂ! Fight!");
    }

    public void ResetArena()
    {
        arenaIsActive = false;

        // Deschide ușa
        if (arenaDoor != null) arenaDoor.SetActive(false);

        // Resetează Boss-ul
        if (bossScript != null) bossScript.ResetToSleep();

        // Aprinde lumina la loc la resetare
        if (globalLight != null) globalLight.intensity = luminaNormala;
        if (playerLight != null) playerLight.enabled = false;
    }

    public void BossDefeated()
    {
        // 1. Deschide ușa
        if (arenaDoor != null) arenaDoor.SetActive(false);

        // 2. Revino la lumina normală
        if (globalLight != null) globalLight.intensity = luminaNormala;

        // 3. Stinge lumina de pe Player
        if (playerLight != null) playerLight.enabled = false;

        // 4. Oprește trigger-ul
        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null) triggerCollider.enabled = false;

        Debug.Log("BOSS ÎNVINS! Porțile s-au deschis.");
    }
}