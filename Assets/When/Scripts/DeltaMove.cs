using System;
using UnityEngine;

namespace CHSF {
	public class DeltaMove : MonoBehaviour {

		public event Action<Vector3> MoveTo;

		[SerializeField] Vector3 delta;

	    Vector3 startPos, endPos;

	    void Start() {
	        startPos = transform.position;
	        endPos = transform.position + delta;
	    }

		public void MoveOnBool(bool b) {
			Vector3 destination = (b ? endPos : startPos);
			if (MoveTo != null) {
				MoveTo(destination);
			} else {
				transform.position = destination;
			}
		}
	}
}