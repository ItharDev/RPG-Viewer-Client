using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowRacer : MonoBehaviour
{
    Camera Camera;
    void Start()
    {
        Camera = GetComponent<Camera>();
    }

    void Update()
    {
        if (RacerController.instance == null) {
            return;
        }

        RacerController.instance.Updates();

        Vector3 currentPosition = transform.position;
        Vector3 newPosition = RacerController.instance.transform.position;

        //currentPosition.x = currentPosition.x * 0.95f + newPosition.x * 0.05f;
        //currentPosition.y = currentPosition.y * 0.9f + (newPosition.y + 20) * 0.1f;

        float speed = RacerController.instance.speed;

        if (speed > 95) {
            speed = 95;
        }

        float speedOffset = speed / 2.5f;


        currentPosition.x = newPosition.x;
        currentPosition.y = newPosition.y + speedOffset;

        transform.position = currentPosition;

        Camera.orthographicSize = 30 + speedOffset / 2;
    }
}
