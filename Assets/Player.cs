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

This script is responsible for managing the player, their movement
*/
public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();   
    }

    private void Update()
    {
        Movement();
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
}
