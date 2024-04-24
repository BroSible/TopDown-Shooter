using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    Rigidbody rb;
    public bool isWalking;
    public Transform orientation;
    Vector3 movement;
    Animator animator;
    public float health;
    public static float playerHealth;
    public float speed;
    public static bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerHealth = health;
    }

    void FixedUpdate()
    {
        Walk();
        if (isWalking)
        {
            animator.Play("Run");
        }

        else
        {
            animator.Play("Idle");
        }

        if(isDead)
        {
            StartCoroutine(C_OnDefeatPlayer());
        }
        
    }

    public void Walk()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        movement = new Vector3(moveHorizontal,0,moveVertical);
        transform.Translate(movement * speed * Time.deltaTime, Space.World);

        if (movement == Vector3.zero) 
        { 
            rb.velocity = Vector3.zero;
            isWalking = false;
        } 

        else
        {
            isWalking = true;
        }
    }

    public static void TakeDamage(float damage)
    {
        playerHealth -= damage;
        if(playerHealth < 0)
        {
            Debug.Log("Ты сдох");
            isDead = true;
        }
    }

    public IEnumerator C_OnDefeatPlayer()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }


}



