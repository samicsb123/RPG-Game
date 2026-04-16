using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Setari Miscare")]
    public float moveSpeed = 5f;

    [Header("Referinte")]
    public Rigidbody2D rb;
    public Animator anim;

    private Vector2 movement;

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        // TINE MINTE DIRECTIA: Actualizăm X și Y doar dacă jucătorul apasă pe taste
        if (movement.x != 0 || movement.y != 0)
        {
            anim.SetFloat("Horizontal", movement.x);
            anim.SetFloat("Vertical", movement.y);
        }

        // Trimitem mereu viteza ca să știe când să treacă din Walk în Idle
        anim.SetFloat("Speed", movement.sqrMagnitude);
    }

    void FixedUpdate()
    {
        rb.velocity = movement * moveSpeed;
    }
}