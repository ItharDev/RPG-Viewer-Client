using UnityEngine;

namespace FunkyCode.Utilities
{
	public class DestroyTimer : MonoBehaviour {
		TimerHelper timer;

		void Start () {
			timer = TimerHelper.Create();
		}
		
		void Update () {
			if (timer.GetMillisecs() > 2000) {
				Destroy(gameObject);
			}
		}
	}
}