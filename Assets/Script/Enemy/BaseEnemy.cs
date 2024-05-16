using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class BaseEnemy : MonoBehaviour
{
    #region Enemy parameters
    [SerializeField] protected float health;
    [SerializeField] protected List<float> cooldowns;
    [SerializeField] protected float detectionDistance;
    [SerializeField] protected List<float> attackDistances;
    #endregion
    [SerializeField] protected AudioClip walkSound; // Используем отдельные переменные для звуков
    [SerializeField] protected AudioClip attackSound;
    [SerializeField] protected AudioClip deathSound;
    [SerializeField] protected AudioClip hitSound; // Звук попадания урона
    [SerializeField] protected AudioClip screamSound; // Звук крика
    private Transform _target;
    [SerializeField]  protected bool _isDead = false;
    protected Animator _animator;
    protected Rigidbody _rb;
    Collider _collider;
    protected AudioSource _audioSource;
    protected NavMeshAgent _agent;
    public LayerMask Ground, Player, Rock;
    protected Vector3 walkPoint;
    [SerializeField] protected bool isWalkPointSet;
    [SerializeField] protected bool isAlreadyAttacked;
    [SerializeField] protected bool playerInSightRange, playerInAttackRange;
    [SerializeField] protected bool hasBeenTargeted;
    [SerializeField] protected float walkPointRange;
    [SerializeField] protected float damage;

    // Массив предметов для спавна
    public GameObject[] bonus;

    // Шанс спавна предмета (в процентах)
    public float spawnBonusChance = 10f;

    #region Events
    //event for attacking beetle
    public delegate void AttackEventHandler();
    public event AttackEventHandler Attack;

    //event for running beetle
    public delegate void RunEventHandler();
    public event RunEventHandler Run;

    //event for death beetle
    public delegate void DeathEventHandler();
    public event DeathEventHandler Death;

    //event for getting hit
    public delegate void HitEventHandler();
    public event HitEventHandler Hit;
    #endregion Events

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>(); 
        _audioSource = GetComponent<AudioSource>(); 
        _agent = GetComponent<NavMeshAgent>(); 
        _rb = GetComponent<Rigidbody>(); 
        _collider = GetComponent<Collider>(); 

        
        _audioSource.loop = true; 
    }
    
    protected virtual void Start()
    {
        if(GameObject.FindWithTag("Player"))
        {
            _target = GameObject.FindWithTag("Player").transform;
        }  

        
        Invoke("PlayScreamSound", 2f); 
    }

    protected virtual void FixedUpdate()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, detectionDistance, Player);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackDistances[0], Player);

        if(!playerInSightRange && !playerInAttackRange && !_isDead)
        {
            Patroling();
        }

        else if(playerInSightRange && !playerInAttackRange && !_isDead)
        {
            ChasePlayer();
            hasBeenTargeted = true;
        }

        else if(playerInSightRange && playerInAttackRange && !_isDead)
        {
            AttackPlayer();
        }

        if(hasBeenTargeted && !playerInSightRange && !playerInAttackRange && !_isDead)
        {
            ChasePlayer();
            StartCoroutine(C_ResetFollowingPlayer());
        }

        if(_isDead)
        {
            _agent.SetDestination(transform.position);
        }
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;

        if(health <= 0)
        {
            Death?.Invoke();
            _isDead = true;
            gameObject.tag = "Untagged";
            Destroy(_collider);
            SpawnRandomBonus();
            StartCoroutine(C_OnDefeat());
        }
        else
        {
            Hit?.Invoke();
        }
    }

    public virtual IEnumerator C_OnDefeat()
    {
        float animationLength = _animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength + 10f);
        Destroy(gameObject);
    }

    protected virtual void SearchWalkPoint()
    {
        NavMeshHit hit;
        NavMeshQueryFilter filter = new NavMeshQueryFilter();
        filter.areaMask = NavMesh.AllAreas & ~Rock.value; // Игнорируем Rock слой
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * walkPointRange;
        if (NavMesh.SamplePosition(randomPoint, out hit, walkPointRange, filter))
        {
            walkPoint = hit.position;
            isWalkPointSet = true;
        }
    }


    protected virtual void Patroling()
    {
        Run?.Invoke();
        if(!isWalkPointSet)
        {
            SearchWalkPoint();
        }

        else
        {
            _agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if(distanceToWalkPoint.magnitude < 1f)
        {
            isWalkPointSet = false;
        }
    }

    protected virtual void ChasePlayer()
    {
        Run?.Invoke();
        _agent.SetDestination(_target.position);
    }

    protected virtual void AttackPlayer()
    {
        _agent.SetDestination(transform.position);
        transform.LookAt(new Vector3(_target.position.x, transform.position.y, _target.position.z));
        
        if(!isAlreadyAttacked)
        {
            Attack?.Invoke();
            Controller.TakeDamage(damage); 
            Debug.Log(Controller.playerHealth);
            isAlreadyAttacked = true;
            Invoke(nameof(ResetAttack),cooldowns[0]); 
        }
    }

    protected virtual void ResetAttack()
    {
        isAlreadyAttacked = false;
    }

    public virtual IEnumerator C_ResetFollowingPlayer()
    {
        yield return new WaitForSeconds(6f);
        hasBeenTargeted = false;
    }

    protected virtual void SpawnRandomBonus()
    {
        float randomNumber = Random.Range(0f,100f);

        if(randomNumber <= spawnBonusChance)
        {
            int randomIndex = Random.Range(0, bonus.Length);
            Instantiate(bonus[randomIndex], transform.position + Vector3.up * 3f, Quaternion.identity);
        }
    }

    
    protected virtual void PlayWalkSound()
    {
        if (walkSound != null)
        {
            _audioSource.clip = walkSound;
            if (!_audioSource.isPlaying) 
            {
                _audioSource.Play(); 
            }
        }
    }

    
    protected virtual void PlayAttackSound()
    {
        if (attackSound != null)
        {
            _audioSource.clip = attackSound;
            _audioSource.loop = false; 
            _audioSource.Play(); 
        }
    }

    
    protected virtual void PlayDeathSound()
    {
        if (deathSound != null)
        {
            _audioSource.clip = deathSound;
            _audioSource.loop = false; 
            _audioSource.Play(); 
        }
    }

    
    protected virtual void PlayHitSound()
    {
        if (hitSound != null)
        {
            _audioSource.PlayOneShot(hitSound); 
        }
    }

    
    protected virtual void PlayScreamSound()
    {
        if (screamSound != null)
        {
            _audioSource.PlayOneShot(screamSound);
        }
    }

    
    protected virtual void OnEnable()
    {
        Attack += PlayAttackSound;
        Run += PlayWalkSound;
        Death += PlayDeathSound;
        Hit += PlayHitSound; 
    }


    protected virtual void OnDisable()
    {
        Attack -= PlayAttackSound;
        Run -= PlayWalkSound;
        Death -= PlayDeathSound;
        Hit -= PlayHitSound; 
    }
    
}