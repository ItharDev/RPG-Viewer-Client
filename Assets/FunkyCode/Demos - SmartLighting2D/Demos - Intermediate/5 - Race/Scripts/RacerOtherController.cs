using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacerOtherController : MonoBehaviour
{
    public enum Type {Forward, Backward}
    public float speed = 0;
    public AudioSource audioSource;
    public Type type = Type.Backward;

    void Start() {
        speed = Random.Range(30, 60);
    }

    void Update() {
        if (RacerController.instance == null) {
            return;
        }

        transform.Translate(0, Time.deltaTime * speed, 0);

        audioSource.pitch = 0.5f + speed / 70;

        float dis= Vector3.Distance(transform.position, RacerController.instance.transform.position);

        if (dis > 100) {
            dis = 100;
        }

        audioSource.volume = (100 - dis) / 100;

        if (type == Type.Backward) {
            float distance = transform.position.y - RacerController.instance.transform.position.y;

            if (distance < -100) {
                transform.Translate(0, Random.Range(-450, -150), 0);
            }

        } else {
            float distance = transform.position.y - RacerController.instance.transform.position.y;
    
            if (distance < -100) {
               transform.Translate(0, Random.Range(150, 450), 0);
            }
        }

    }
}
