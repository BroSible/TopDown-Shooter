using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour
{
    Rigidbody _rb;
    public bool isWalking;
    public Transform orientation;
    Vector3 movement;
    Animator _animator;
    public float health;
    public static float playerHealth;
    public float speed;
    public static bool isDead = false;
    CameraCursor _cameraCursor;
    AudioSource _audioSource;
    public AudioClip _footStep;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _cameraCursor = GetComponent<CameraCursor>();
        playerHealth = health;
        _audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        
        if (!isDead)
        {
            Walk();
        }

        else if(!isDead && !isWalking)
        {
            _animator.Play("Idle");

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
        transform.Translate(movement * speed * Time.deltaTime,Space.World);

        if (movement == Vector3.zero && !isDead) 
        { 
            _rb.velocity = Vector3.zero;
            isWalking = false;
            _animator.Play("Idle");
            _audioSource.clip = null;
        } 

        else
        {
            _animator.Play("Run");
            isWalking = true;
        }
    }

    public static void TakeDamage(float damage)
    {
        playerHealth -= damage;
        if(playerHealth <= 0)
        {

            isDead = true;
        }
    }

    public IEnumerator C_OnDefeatPlayer()
    {
        _cameraCursor.enabled = false;
        _animator.Play("Dead1");
        
        yield return new WaitForSeconds(2f);
        isDead = false;
        SceneManager.LoadScene("SampleScene"); 
    }


}



