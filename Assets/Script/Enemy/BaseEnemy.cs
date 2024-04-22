using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [SerializeField] protected int health;
    [SerializeField] protected List<float> cooldowns;
    [SerializeField] protected List<float> detectionDistances;
    [SerializeField] protected List<float> attackDistances;
    public Transform _target;
    protected bool _isDead=false;
    protected Animator _animator;
    
    protected virtual void Start()
    {
        if(GameObject.FindWithTag("Player"))
        {
            _target = GameObject.FindWithTag("Player").transform;
        }  
    }

    void Update()
    {
        
    }
}
