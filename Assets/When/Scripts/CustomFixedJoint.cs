using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CustomFixedJoint : MonoBehaviour {
    Collider _collider;

    FixedJoint _fixedJoint;
    readonly List<Collider> _ignoredColliders = new List<Collider>();

    void Awake() {
        _collider = GetComponent<Collider>();
    }

    public void Apply(Rigidbody rigidbody, Collider collider) {
        _fixedJoint = gameObject.AddComponent<FixedJoint>();
        _fixedJoint.connectedBody = rigidbody;
        //_fixedJoint.breakForce = 5f; // TODO check how use it

        if (collider != null) {
            _ignoredColliders.Add(collider);
            Physics.IgnoreCollision(_collider, collider, true);
        }
    }

    void OnDestroy() {
        Destroy(_fixedJoint);
        _ignoredColliders.ForEach(c => {
            if (c != null)
                Physics.IgnoreCollision(_collider, c, false);
        });
    }
}
