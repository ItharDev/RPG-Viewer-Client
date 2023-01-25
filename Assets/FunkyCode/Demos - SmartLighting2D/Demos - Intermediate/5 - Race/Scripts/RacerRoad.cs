using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacerRoad : MonoBehaviour
{
    public static List<RacerRoad> list = new List<RacerRoad>();

   	public void OnEnable() {
		list.Add(this);
	}

    void Update() {
        if (RacerController.instance == null) {
            return;
        }
        
        float distance = Mathf.Abs(transform.position.y - RacerController.instance.transform.position.y);

        if (distance > 80) {
            transform.Translate(0, 64 * 2, 0);
        }
    }
}
