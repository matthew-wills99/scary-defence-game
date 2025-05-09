using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
#################################################
# ____  _        _ __   _______ ____            #
#|  _ \| |      / \\ \ / / ____|  _ \   ___ ___ #
#| |_) | |     / _ \\ V /|  _| | |_) | / __/ __|#
#|  __/| |___ / ___ \| | | |___|  _ < | (__\__ \#
#|_|   |_____/_/   \_\_| |_____|_| \_(_)___|___/#
#################################################

This script is responsible for managing the player, their movement, and their actions
*/
public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool canJump;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Interact")]
    public float reachLength = 2f;

    [Header("")]

    public Transform orientation;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;
    private Rigidbody rb;

    private void Start()
    {
        canJump = true;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);

        GetInput();
        SpeedControl();

        if(isGrounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0f;
        }

        Interact();
    }

    private void FixedUpdate()
    {
        Movement();   
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetButton("Jump") && canJump && isGrounded)
        {
            canJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void Movement()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * airMultiplier * 10, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        canJump = true;
    }

    private void Interact()
    {
        Debug.DrawLine(transform.position, transform.position + transform.forward * reachLength, Color.red, 5f);
        Ray ray = new Ray(transform.position, transform.forward);
        if(Physics.Raycast(ray, out RaycastHit hit, reachLength))
        {
            if(hit.collider.CompareTag("Interactive"))
            {
                if(hit.collider.TryGetComponent<InteractiveObject>(out var interactiveObject))
                {
                    interactiveObject.Interact();
                }
            }
        }
    }
}
