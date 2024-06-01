using UnityEngine;
using System.Collections;

namespace CHSF {
	public class MoveSmoothly : MonoBehaviour {

		[SerializeField] float _duration = 1;
		[SerializeField] AnimationCurve _speedCurve;

		void Start() { } // can be disabled in editor

		public void Move(Vector3 endPos) {
            StopAllCoroutines();
			StartCoroutine(Animate(endPos));
		}

		IEnumerator Animate(Vector3 endPos) {
			Vector3 startPos = transform.position;

			float timer = 0f;
			while (timer <= _duration) {
				transform.position = Vector3.Lerp(startPos, endPos, _speedCurve.Evaluate(timer / _duration));
				timer += Time.deltaTime;
				yield return null;
			}
		}
	}
}
