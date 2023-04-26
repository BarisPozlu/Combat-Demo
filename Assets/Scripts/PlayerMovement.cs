using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 5;
    private float horizontalMovement = 0;

    private Animator animator;
    private Rigidbody2D playerBody;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        playerBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        CorrectFaceDirection();
        HandleMovement();
    }

    private void HandleMovement()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        playerBody.velocity = new Vector2(horizontalMovement * movementSpeed, playerBody.velocity.y);

        if (horizontalMovement == 0)
        {
            animator.SetBool("Walk", false);
        }

        else
        {
            animator.SetBool("Walk", true);
        }
    }

    private void CorrectFaceDirection()
    {
        if ((horizontalMovement == 1 && !Player.facingRight) || (horizontalMovement == -1 && Player.facingRight))
        {
            Rotate180();
        }
    }

    private void Rotate180()
    {
        transform.Rotate(Vector3.up, 180);
        Player.facingRight = !Player.facingRight;
    }
}