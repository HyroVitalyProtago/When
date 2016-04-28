/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2014.                                   *
* Leap Motion proprietary. Licensed under Apache 2.0                           *
* Available at http://www.apache.org/licenses/LICENSE-2.0.html                 *
\******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using When;
using When.Interfaces;

// TODO don't collide on release
public class GrabbableObject : MonoBehaviour {

    [SerializeField] Collider collider;

    public bool IsGrabbed { get; protected set; }
    public bool IsHovered { get; protected set; }
    
    Dictionary<GrabDetector, int> _grabDetectors = new Dictionary<GrabDetector, int>();
    Vector3 origPos, deltaPos;

    public virtual void OnStartHover() {
        IsHovered = true;
    }

    public virtual void OnStopHover() {
        IsHovered = false;
    }

    public virtual void OnGrab(ITransform iTransform) {
        IsGrabbed = true;
        IsHovered = false;
        StopAllCoroutines();
        //Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        //if (rigidbody != null) Destroy(rigidbody);
        StartCoroutine(Grab(iTransform));
    }

    IEnumerator Grab(ITransform iTransform) {
        origPos = transform.position;
        deltaPos = iTransform.Position;
        while (IsGrabbed) {
            transform.position = origPos + iTransform.Position - deltaPos;
            transform.rotation = iTransform.Rotation;
            yield return null;
        }
    }

    public virtual void OnRelease(ITransform iTransform) {
        IsGrabbed = false;
        //gameObject.AddComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other) {
        GrabDetector grabDetector = other.gameObject.GetComponentInParent<GrabDetector>();
        if (grabDetector != null) {
            if (!_grabDetectors.ContainsKey(grabDetector)) {
                _grabDetectors.Add(grabDetector, 0);
                grabDetector.OnFinish += (ITransform iT) => {
                    OnRelease(iT);
                }; // TODO better... (object is released when a hand finish grab, even if it's not the good one...)
            }
            if (++_grabDetectors[grabDetector] == 1) {
                OnStartHover();
                grabDetector.OnBegin += OnGrab;
            }
        }
    }

    void OnTriggerExit(Collider other) {
        GrabDetector grabDetector = other.gameObject.GetComponentInParent<GrabDetector>();
        if (grabDetector != null) {
            if (--_grabDetectors[grabDetector] == 0) {
                OnStopHover();
                grabDetector.OnBegin -= OnGrab;
                //grabDetector.OnFinish -= OnRelease;
            }
        }
    }
}
