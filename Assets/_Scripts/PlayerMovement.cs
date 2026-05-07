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
        // Preluăm input-ul de la tastatură
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Normalizăm vectorul ca viteza să fie constantă pe diagonală
        movement = movement.normalized;

        // TINE MINTE DIRECTIA: Actualizăm parametrii din Animator doar dacă ne mișcăm
        // Asta previne resetarea caracterului cu fața în sus când iei mâna de pe taste
        if (movement.x != 0 || movement.y != 0)
        {
            anim.SetFloat("Horizontal", movement.x);
            anim.SetFloat("Vertical", movement.y);
        }

        // Trimitem viteza către Animator (pentru a trece între Idle și Walk)
        anim.SetFloat("Speed", movement.sqrMagnitude);
    }

    void FixedUpdate()
    {
        // Mișcăm fizic obiectul folosind Rigidbody
        rb.velocity = movement * moveSpeed;
    }
}