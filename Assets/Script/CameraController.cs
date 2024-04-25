using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 newPosition;
    [SerializeField] private Vector3 targetOffset;
    [SerializeField] private float movementSpeed;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        Vector3 targetPosition = target.position + targetOffset;
        newPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movementSpeed);
        transform.position = newPosition;
    }

}

    

