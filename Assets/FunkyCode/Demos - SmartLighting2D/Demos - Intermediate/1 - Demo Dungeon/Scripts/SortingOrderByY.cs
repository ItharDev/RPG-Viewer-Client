using UnityEngine;

[ExecuteInEditMode]
public class SortingOrderByY : MonoBehaviour
{
    public int sortingOrder = 0;
    public SpriteRenderer spriteRenderer;
    
    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        sortingOrder = -(int)(transform.position.y * 10);

        spriteRenderer.sortingOrder = sortingOrder;
    }
}
