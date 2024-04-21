using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCursor : MonoBehaviour
{
   private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera != null)
        {
            Vector3 cursorPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.y - transform.position.y));

            Vector3 direction = cursorPosition - transform.position;

            direction.y = 0;

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }
    }

} 

