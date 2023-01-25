using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonPlayerController : MonoBehaviour
{
 
    void Start()
    {
        
    }

 
    void Update()
    {
        Vector3 position = transform.position;
        float speed = Time.deltaTime * 4;

        if (Input.GetKey(KeyCode.W)) {
            position.y += speed;
        }

        if (Input.GetKey(KeyCode.S)) {
           position.y -= speed; 
        }

        if (Input.GetKey(KeyCode.A)) {
            position.x -= speed;
        }

         if (Input.GetKey(KeyCode.D)) {
            position.x += speed;
        }

        transform.position = position;
    }
}
