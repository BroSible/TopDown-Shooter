using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
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
    public static int score = 0;
    public static bool isDead = false;
    CameraCursor _cameraCursor;
    AudioSource _audioSource;
    public AudioClip _footStep;
    public float footstepInterval = 0.5f;
    private float lastFootstepTime = 0f;
    public AudioClip _damageSound; // Звук получения урона
    public AudioClip _deathSound; // Звук смерти
    public GameObject mainCamera;
    public GameObject deathCamera;
    private BaseWeapon _baseWeapon;
    public PauseMenu pauseMenu;

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
            _baseWeapon = GetComponentInChildren<BaseWeapon>();
            pauseMenu.enabled = false;
            StartCoroutine(C_OnDefeatPlayer());
        }
        
    }

    public void Walk()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        movement = new Vector3(moveHorizontal, 0, moveVertical);

        if (movement == Vector3.zero && !isDead)
        {
            _rb.velocity = Vector3.zero;
            isWalking = false;
            _animator.Play("Idle");
            _audioSource.clip = null;
        }

        else
        {
            _rb.MovePosition(transform.position + movement * speed * Time.deltaTime);
            
            _animator.Play("Run");
            isWalking = true;

            // Проигрываем звук шагов с интервалом
            if (Time.time - lastFootstepTime > footstepInterval)
            {
                _audioSource.PlayOneShot(_footStep);
                lastFootstepTime = Time.time;
            }
        }
    }


    public static void TakeDamage(float damage)
    {
        Debug.Log($"Игрок получил {damage} урона."); // Добавлено: лог урона
        playerHealth -= damage;
        if(playerHealth <= 0)
        {
            isDead = true;
            Debug.Log("Игрок умер!"); // Добавлено: лог смерти
        }
        else
        {
            // Проигрываем звук получения урона
            FindObjectOfType<Controller>()._audioSource.PlayOneShot(FindObjectOfType<Controller>()._damageSound); 
        }
    }

    public IEnumerator C_OnDefeatPlayer()
    {
        // Проигрываем звук смерти только один раз
        if (!_audioSource.isPlaying)
        {
            _audioSource.PlayOneShot(_deathSound);
        }

        _baseWeapon.enabled = false;
        _cameraCursor.enabled = false;
        _animator.Play("Dead1");
        gameObject.tag = "Untagged";
        yield return new WaitForSeconds(2f);
        
        this.gameObject.SetActive(false);
        mainCamera.SetActive(false);
        deathCamera.SetActive(true);
    }

    public void StopAudio()
    {
        _audioSource.Stop();
    }
}