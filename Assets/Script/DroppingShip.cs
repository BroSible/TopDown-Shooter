using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppingShip : MonoBehaviour
{
    // Start is called before the first frame update
public Animator animator;

    void Start()
    {
        animator.SetTrigger("Appear");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
