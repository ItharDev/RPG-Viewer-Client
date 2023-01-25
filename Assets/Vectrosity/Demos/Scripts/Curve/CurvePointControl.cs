using UnityEngine;
using UnityEngine.EventSystems;

public class CurvePointControl : MonoBehaviour, IDragHandler {

	public int objectNumber;
 	
	public void OnDrag (PointerEventData eventData) {
		transform.position = Input.mousePosition;
		DrawCurve.use.UpdateLine (objectNumber, Input.mousePosition);
	}
}