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

        // Regula de aur: Trimitem date la Animator DOAR când ne mișcăm
        if (moveInput != Vector2.zero)
        {
            animator.SetFloat("Horizontal", moveInput.x);
            animator.SetFloat("Vertical", moveInput.y);

            // Reținem direcția pentru atac
            lastMoveDir = moveInput.normalized;
        }
        // Când moveInput e zero (adică stai), NU mai scriem nimic în animator.
        // Așa el rămâne pe ultima valoare primită (stânga, sus, etc.)
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput.normalized * speed * Time.fixedDeltaTime);
    }
}