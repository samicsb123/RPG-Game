using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Adãugat pentru View-ul de UI
using TMPro;          // Adãugat pentru Text

public class GiantSlimeBoss : MonoBehaviour
{
    private enum Phase { Standard, Mutated }
    private Phase currentPhase = Phase.Standard;

    [Header("Health Pool (The Model)")]
    public int maxHealth = 300;
    public int currentHealth;

    [Header("UI References (The View)")]
    public Slider bossHealthSlider;
    public TextMeshProUGUI bossNameText;

    [Header("Animation Swapping")]
    public RuntimeAnimatorController phase1Controller;
    public RuntimeAnimatorController phase2Controller;

    [Header("Physics & Kinematics")]
    public float standardHopForce = 6f;
    public float mutatedDashForce = 18f;
    public float hopCooldownPhase1 = 1.8f;
    public float hopCooldownPhase2 = 0.8f;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform playerTransform;

    private bool isActing = false;
    private int phase2HopCounter = 0;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (phase1Controller != null) animator.runtimeAnimatorController = phase1Controller;

        // Ini?ializare View (UI)
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
            if (playerTransform == null || isActing)
            {
                yield return null;
                continue;
            }

            if (currentPhase == Phase.Standard)
            {
                yield return StartCoroutine(ExecuteHop(standardHopForce));
                yield return new WaitForSeconds(hopCooldownPhase1);
            }
            else if (currentPhase == Phase.Mutated)
            {
                phase2HopCounter++;
                if (phase2HopCounter >= 3)
                {
                    phase2HopCounter = 0;
                    yield return StartCoroutine(ExecuteDeadlyDash());
                }
                else
                {
                    yield return StartCoroutine(ExecuteHop(standardHopForce * 1.2f));
                }
                yield return new WaitForSeconds(hopCooldownPhase2);
            }
        }
    }

    private IEnumerator ExecuteHop(float force)
    {
        isActing = true;
        animator.SetTrigger("Jump");
        Vector2 targetDir = (playerTransform.position - transform.position).normalized;
        yield return new WaitForSeconds(0.1f);
        rb.AddForce(targetDir * force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.4f);
        rb.velocity = Vector2.zero;
        isActing = false;
    }

    private IEnumerator ExecuteDeadlyDash()
    {
        isActing = true;
        Vector2 lockedTrajectory = (playerTransform.position - transform.position).normalized;
        yield return new WaitForSeconds(0.6f);
        animator.SetTrigger("Jump");
        rb.AddForce(lockedTrajectory * mutatedDashForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.35f);
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(1.2f);
        isActing = false;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        animator.SetTrigger("Hit");

        // PUSH DATA TO UI INSTANTLY
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

        if (bossNameText != null) bossNameText.text = "THE GIANT GELATIN (Enraged!)";
        bossNameText.color = Color.red;

        animator.SetTrigger("Hit");
        yield return new WaitForSeconds(0.4f);
        if (phase2Controller != null) animator.runtimeAnimatorController = phase2Controller;

        rb.AddForce(Vector2.up * 3f, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.8f);
        isActing = false;
    }

    private void Die()
    {
        StopAllCoroutines();
        rb.velocity = Vector2.zero;
        animator.SetTrigger("Hit");

        // Ascundem bara de via?ã când moare
        if (bossHealthSlider != null) bossHealthSlider.gameObject.SetActive(false);

        Destroy(gameObject, 0.5f);
    }
}