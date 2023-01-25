using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunkyCode.Utilities;

namespace FunkyCode
{
    public class DemoLightColorChange : MonoBehaviour
    {
        public float speed = 1f;
        public float time = 1f;
        public Color[] colors;

        int currentID = 0;
        TimerHelper timer;
        Light2D lightSource;

        void Start() {
            timer = TimerHelper.Create();

            lightSource = GetComponent<Light2D>();
        }

        void Update() {
            Color color = lightSource.color;
            Color newColor = colors[currentID];

            color.r = Mathf.Lerp(color.r, newColor.r, Time.deltaTime * speed);
            color.g = Mathf.Lerp(color.g, newColor.g, Time.deltaTime * speed);
            color.b = Mathf.Lerp(color.b, newColor.b, Time.deltaTime * speed);

            lightSource.color = color;

            if (timer.Get() > time) {
                timer.Reset();

                currentID += 1;

                if (currentID >= colors.Length) {
                    currentID = 0;
                }
            }
        }
    }
}