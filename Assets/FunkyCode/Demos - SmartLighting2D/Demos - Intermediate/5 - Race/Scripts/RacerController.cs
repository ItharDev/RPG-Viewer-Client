using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacerController : MonoBehaviour
{
    public static RacerController instance;
    public AudioSource audioSource;
    public float speed = 0;

    void Start() {
        instance = this;
    }

    public void Updates()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetMouseButton(0)) {
            speed = speed * 0.98f + 100 * 0.02f;
        } else if (Input.GetKey(KeyCode.Space)) {
            speed = speed * 0.98f + 150 * 0.02f;
        } else {
            speed = speed * 0.98f;
        }

        if (Input.GetKey(KeyCode.D)) {
            transform.Rotate(0, 0, - Time.deltaTime * 45);

        }

         if (Input.GetKey(KeyCode.A)) {
            transform.Rotate(0, 0, Time.deltaTime* 45);

        }

        audioSource.pitch = 0.5f + speed / 70;

        transform.Translate(0, Time.deltaTime * speed, 0);
    }
}
