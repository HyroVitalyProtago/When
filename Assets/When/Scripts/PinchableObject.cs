using System;
using UnityEngine;

/*
 * TODO: Catchable -> (Pinchable, Grabbable) -> ...?
 */

namespace When {
    public class PinchableObject : MonoBehaviour {
        //[SerializeField] Collider collider;

        public event Action<bool> OnHeldChange;

        bool _isHeld;

        public bool IsHeld {
            get {
                return _isHeld;
            }
            set {
                if (value != _isHeld) {
                    _isHeld = value;
                    if (OnHeldChange != null) {
                        OnHeldChange(_isHeld);
                    }
                }
            }
        }

        //public bool IsHovered { get; protected set; }

        //void OnTriggerEnter(Collider collider) {
        //    print("Enter");
        //    print(collider);
        //}

        //void OnTriggerExit(Collider collider) {
        //    print("Exit");
        //    print(collider);
        //}
    }
}