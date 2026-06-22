using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GiantSlimeBoss : MonoBehaviour
{
    private enum Phase { Standard, Mutated }
    private Phase currentPhase = Phase.Standard;

    [Header("Arena System")]
    public bool isAsleep = true;
    private Vector2 startPosition; // Memorează unde e centrul camerei

    [Header("Health Pool")]
    public int maxHealth = 300;
    public int currentHealth;

    [Header("UI References")]
    public Slider bossHealthSlider;
    public TextMeshProUGUI bossNameText;

    [Header("Animation Swapping")]
    public RuntimeAnimatorController phase1Controller;
    public RuntimeAnimatorController phase2Controller;

    [Header("Phase 2 Weapons")]
    public GameObject projectilePrefab;
    public GameObject miniSlimeClonePrefab;
    public Transform firePoint;
    public float projectileSpeed = 8f;
    public int numberOfBullets = 30;

    [Header("Jump Targeting System")]
    // Cât timp stă efectiv "în aer" până aterizează pe player
    public float phase1FlightTime = 0.8f;  // Mai încet, greoi
    public float phase2FlightTime = 0.4f;  // Rapid, agresiv (pe jumătate)

    // Cât stă degeaba pe loc între atacuri
    public float hopCooldownPhase1 = 1.8f;
    public float hopCooldownPhase2 = 1.0f;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform playerTransform;

    private bool isActing = false;
    private int phase2AttackCycle = 0;

    void Start()
    {
        startPosition = transform.position;
        if (bossHealthSlider != null) bossHealthSlider.gameObject.SetActive(false); // Ascundem bara de viață până începe lupta

        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (phase1Controller != null) animator.runtimeAnimatorController = phase1Controller;

        if (bossHealthSlider != null)
        {
            bossHealthSlider.maxValue = maxHealth;
            bossHealthSlider.value = currentHealth;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        StartCoroutine(BossAI_Loop());
    }

    private IEnumerator BossAI_Loop()
    {
        while (currentHealth > 0)
        {
            // Dacă boss-ul doarme (nu ai intrat în arenă), doar așteaptă
            if (isAsleep)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            if (playerTransform == null || isActing)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            if (currentPhase == Phase.Standard)
            {
                // În faza 1 face doar sărituri clasice, mai lente
                yield return StartCoroutine(ExecuteTargetedJump(phase1FlightTime));
                yield return new WaitForSeconds(hopCooldownPhase1);
            }
            else if (currentPhase == Phase.Mutated)
            {
                // Alege un atac random din cele 4 disponibile (0, 1, 2, 3)
                int randomAttack = Random.Range(0, 4);

                switch (randomAttack)
                {
                    case 0:
                        yield return StartCoroutine(ExecuteTargetedJump(phase2FlightTime));
                        break;
                    case 1:
                        yield return StartCoroutine(ShootProjectile());
                        break;
                    case 2:
                        yield return StartCoroutine(SpawnClones());
                        break;
                    case 3:
                        yield return StartCoroutine(ExecuteTargetedJump(0.2f)); // Dash-ul rapid
                        break;
                }

                // După fiecare atac, așteaptă cooldown-ul specificat
                yield return new WaitForSeconds(hopCooldownPhase2);
            }
        }
    }

    /// <summary>
    /// Calculează viteza necesară pentru a ajunge EXACT pe poziția playerului într-un timp dat.
    /// Indiferent dacă e la 2 metri sau la 10 metri, aterizează mereu la țintă.
    /// </summary>
    private IEnumerator ExecuteTargetedJump(float flightTime)
    {
        if (playerTransform == null) yield break;
        isActing = true;

        animator.SetTrigger("Jump");

        // Memorează unde era player-ul în clipa în care a început săritura
        Vector2 startPos = transform.position;
        Vector2 targetPos = playerTransform.position;

        yield return new WaitForSeconds(0.1f); // Mica întârziere să se potrivească cu frame-ul de animație

        // MATEMATICA: Viteza = Distanța / Timp. 
        // Dacă e departe, viteza rezultată va fi uriașă. Dacă e aproape, va sări încet.
        Vector2 distanceVector = targetPos - startPos;
        Vector2 requiredVelocity = distanceVector / flightTime;

        // Aplicăm viteza brutal, ignorând masa (chiar dacă e 1000)
        rb.velocity = requiredVelocity;

        // Așteptăm exact timpul de zbor cerut
        yield return new WaitForSeconds(flightTime);

        // FRÂNA pe loc la aterizare
        rb.velocity = Vector2.zero;

        isActing = false;
    }

    private IEnumerator ShootProjectile()
    {
        if (firePoint == null || projectilePrefab == null) yield break;
        isActing = true;

        animator.SetTrigger("Jump");
        yield return new WaitForSeconds(0.2f);

        // Luăm collider-ul Boss-ului o singură dată înainte de buclă ca să salvăm memorie
        Collider2D bossCollider = GetComponent<Collider2D>();

        for (int i = 0; i < numberOfBullets; i++)
        {
            if (playerTransform == null) break;

            Vector2 directionToPlayer = (playerTransform.position - firePoint.position).normalized;
            float exactAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            float tinySpread = Random.Range(-2f, 2f);
            float finalAngle = exactAngle + tinySpread;

            Vector2 shootDir = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));

            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // ========================================================
            // REZOLVARE BLOCAJ: Ignorăm coliziunea dintre Glonț și Boss
            // ========================================================
            Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
            if (bossCollider != null && bulletCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, bossCollider);
            }
            // ========================================================

            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null) bulletRb.velocity = shootDir * projectileSpeed;

            yield return new WaitForSeconds(0.06f);
        }

        yield return new WaitForSeconds(0.2f);

        isActing = false;
    }

    private IEnumerator SpawnClones()
    {
        if (miniSlimeClonePrefab == null) yield break;
        isActing = true;

        animator.SetTrigger("Jump");
        yield return new WaitForSeconds(0.2f);

        Instantiate(miniSlimeClonePrefab, transform.position + new Vector3(-1.5f, 0, 0), Quaternion.identity);
        Instantiate(miniSlimeClonePrefab, transform.position + new Vector3(1.5f, 0, 0), Quaternion.identity);

        yield return new WaitForSeconds(0.5f);
        isActing = false;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        animator.SetTrigger("Hit");

        if (bossHealthSlider != null) bossHealthSlider.value = currentHealth;

        if (currentPhase == Phase.Standard && currentHealth <= (maxHealth / 2))
        {
            StartCoroutine(TriggerMutation());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator TriggerMutation()
    {
        isActing = true;
        currentPhase = Phase.Mutated;
        rb.velocity = Vector2.zero;

        if (bossNameText != null)
        {
            bossNameText.text = "THE GIANT GELATIN (Mutated!)";
            bossNameText.color = Color.green;
        }

        animator.SetTrigger("Hit");
        yield return new WaitForSeconds(0.4f);

        if (phase2Controller != null) animator.runtimeAnimatorController = phase2Controller;

        // O mică zvâcnire mecanică la transformare
        rb.velocity = Vector2.up * 3f;
        yield return new WaitForSeconds(0.2f);
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.6f);
        isActing = false;
    }

    private void Die()
    {
        StopAllCoroutines();
        rb.velocity = Vector2.zero;
        animator.SetTrigger("Hit");

        if (bossHealthSlider != null) bossHealthSlider.gameObject.SetActive(false);

        Destroy(gameObject, 0.5f);
    }

    // ==========================================
    // FUNȚIILE APELATE DE ARENA MANAGER (NOU)
    // ==========================================
    public void WakeUpBoss()
    {
        isAsleep = false;
        if (bossHealthSlider != null) bossHealthSlider.gameObject.SetActive(true); // Afișăm bara!
    }

    public void ResetToSleep()
    {
        StopAllCoroutines(); // Oprim orice glonț sau săritură

        isAsleep = true;
        currentHealth = maxHealth;
        currentPhase = Phase.Standard; // Îl scoatem din faza verde
        transform.position = startPosition; // Îl punem înapoi la locul lui

        if (rb != null) rb.velocity = Vector2.zero;
        if (phase1Controller != null && animator != null) animator.runtimeAnimatorController = phase1Controller;

        if (bossHealthSlider != null)
        {
            bossHealthSlider.value = maxHealth;
            bossHealthSlider.gameObject.SetActive(false);
        }

        if (bossNameText != null)
        {
            bossNameText.text = "THE GIANT SLIME";
            bossNameText.color = Color.white;
        }

        StartCoroutine(BossAI_Loop()); // Repornim procesorul intern
    }
}