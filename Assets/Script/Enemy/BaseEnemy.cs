using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemy : MonoBehaviour
{
    #region Enemy parameters
    [SerializeField] protected float health;
    [SerializeField] protected List<float> cooldowns;
    [SerializeField] protected float detectionDistance;
    [SerializeField] protected List<float> attackDistances;
    #endregion
    [SerializeField] protected List<AudioClip> audioClips;
    public Transform _target;
    [SerializeField]  protected bool _isDead = false;
    protected Animator _animator;
    protected Rigidbody _rb;
    protected AudioSource _audioSource;
    protected NavMeshAgent _agent;
    public LayerMask Ground, Player;
    protected Vector3 walkPoint;
    [SerializeField] protected bool isWalkPointSet;
    [SerializeField] protected bool isAlreadyAttacked;
    [SerializeField] protected bool playerInSightRange, playerInAttackRange;
    [SerializeField] protected bool hasBeenTargeted;
    [SerializeField] protected float walkPointRange;
    [SerializeField] protected float damage;

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
    #endregion Events

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _agent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
    }
    
    protected virtual void Start()
    {
        if(GameObject.FindWithTag("Player"))
        {
            _target = GameObject.FindWithTag("Player").transform;
        }  
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
            StartCoroutine(C_OnDefeat());
        }
    }

    public virtual IEnumerator C_OnDefeat()
    {
        float animationLength = _animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength + 10f);
        //animationLength = _animator.GetCurrentAnimatorStateInfo(0).length; тут написано получение длины анимации дял исчезновения врага.
        //yield return new WaitForSeconds(animationLength);
        Destroy(gameObject);
    }

    protected virtual void SearchWalkPoint()
    {
        float walkRandomZ = Random.Range(-walkPointRange, walkPointRange);
        float walkRandomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3 (transform.position.x + walkRandomX,transform.position.y, transform.position.z + walkRandomZ);

        if(Physics.Raycast(walkPoint,-transform.up,10f, Ground))
        {
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
}
