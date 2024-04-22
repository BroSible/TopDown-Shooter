using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{

Rigidbody rb;
public bool iswalking;
public Transform orientation;
Vector3 movement;
Animator animator;
public void walk()
{
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        movement = new Vector3(moveHorizontal,0,moveVertical);
        transform.Translate(movement * speed * Time.deltaTime, Space.World);
       // rb.AddForce(movement.normalized * speed * 5f, ForceMode.Force); 
        if (movement == Vector3.zero) 
        { 
            rb.velocity = Vector3.zero;
            iswalking = false;
        } 
        else
        {
            iswalking = true;
        }
}

public float speed ;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

    }

    
    void FixedUpdate()
    {
        walk();
        if (iswalking)
        {
            animator.Play("Run");
        }

        else
        {
            animator.Play("Idle");
        }
        
    }
}



