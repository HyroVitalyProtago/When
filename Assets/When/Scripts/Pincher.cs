using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using When;
using When.Interfaces;

public class Pincher : MonoBehaviour {
    [SerializeField] float _impulseFactor = 3f;

    PinchableObject _obj;
    
    public void Begin(ITransform it) {
        var objs = FindObjectsOfType<PinchableObject>().ToList();
        if (objs.Count == 0) return;
        
        // get the nearest pinchable object
        foreach (var po in objs) {
            if (_obj == null ||
                Vector3.Distance(po.transform.position, it.Position) <
                Vector3.Distance(_obj.transform.position, it.Position)) {
                _obj = po;
            }
        }
        
        if (Vector3.Distance(_obj.transform.position, it.Position) > .03f) {
            _obj = null;
            return;
        }
        
        DestroyJoints(_obj.gameObject); // todo check if useful
        var child = transform.GetChild(0);
        _obj.gameObject.AddComponent<CustomFixedJoint>().Apply(child.GetComponent<Rigidbody>(), child.GetComponent<Collider>());
    }

    void DestroyJoints(GameObject go) {
        go.GetComponents<CustomFixedJoint>().ToList().ForEach(Destroy);
    }

    public void Finish(ITransform it) {
        if (_obj != null) {
            DestroyJoints(_obj.gameObject);
            StartCoroutine(Impulse(_obj.GetComponent<Rigidbody>(), it.DeltaPosition * _impulseFactor)); // Start a coroutine because DestroyJoints isn't totaly applied
            _obj = null;
        }
    }

    IEnumerator Impulse(Rigidbody rigidbody, Vector3 force) {
        yield return null;
        rigidbody.AddForce(force, ForceMode.Impulse);
    }
}
