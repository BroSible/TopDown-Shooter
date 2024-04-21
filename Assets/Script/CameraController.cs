using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset;
    [SerializeField] private float movementSpeed;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        Vector3 targetPosition = target.position + targetOffset;
        Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movementSpeed);
        transform.position = newPosition;
    }

}

    

