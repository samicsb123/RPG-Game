using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    public Animator animator;

    [HideInInspector] public Vector2 lastMoveDir = Vector2.down;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Regula de aur: Trimitem DIRECTIA la Animator DOAR când ne mișcăm
        if (moveInput != Vector2.zero)
        {
            animator.SetFloat("Horizontal", moveInput.x);
            animator.SetFloat("Vertical", moveInput.y);

            // Reținem direcția pentru atac
            lastMoveDir = moveInput.normalized;
        }

        // --- LINIILE MAGICE ADĂUGATE ---
        // Trimitem viteza de mișcare (va fi mai mare ca 0 când mergi și fix 0 când te oprești)
        animator.SetFloat("Speed", moveInput.sqrMagnitude);

        // În caz că folosești și bifa în Animator, o bifăm/debifăm automat
        animator.SetBool("isWalking", moveInput != Vector2.zero);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput.normalized * speed * Time.fixedDeltaTime);
    }

    public bool isInSafeZone = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Când intri în zona verde a satului
        if (other.CompareTag("SafeZone"))
        {
            isInSafeZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Când ieși din sat
        if (other.CompareTag("SafeZone"))
        {
            isInSafeZone = false;
        }
    }
}

