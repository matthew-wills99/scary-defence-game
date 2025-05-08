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
    [SerializeField] public float speed = 5f;
    [SerializeField] public float gravity = -9.81f;
    [SerializeField] public float reachLength; // for 

    private CharacterController controller;
    private Vector3 velocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();   
    }

    private void Update()
    {
        Movement();
        Interact();
    }

    private void Movement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        controller.Move(move * speed * Time.deltaTime);

        if(controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
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
