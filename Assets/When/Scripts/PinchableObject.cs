using UnityEngine;
using When.Interfaces;

/*
 * TODO: Catchable -> (Pinchable, Grabbable) -> ...?
 */

namespace When {
    public class PinchableObject : MonoBehaviour {
        [SerializeField] Collider collider;

        public bool IsHeld { get; protected set; }
        public bool IsHovered { get; protected set; }

        public virtual void Hold(ITransform iTransform) {
            IsHeld = true;
            IsHovered = false;
        }

        public virtual void OnRelease(ITransform iTransform) {
            IsHeld = false;
        }

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