using UnityEngine;

namespace FunkyCode
{
    [ExecuteInEditMode]
    public class BasicEvents : MonoBehaviour
    {
        SpriteRenderer spriteRenderer;

        void Awake() {
            LightCollider2D collider = GetComponent<LightCollider2D>();

            collider.AddEventOnEnter(OnEnter);
            collider.AddEventOnExit(OnExit);

            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void OnEnter(Light2D light) {
            spriteRenderer.color = Color.red;
        }

        void OnExit(Light2D light) {
            spriteRenderer.color = Color.green;
        }
    }
}