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

    public bool IsGrabbed { get; protected set; }
    public bool IsHovered { get; protected set; }
    
    Dictionary<GrabDetector, int> _grabDetectors = new Dictionary<GrabDetector, int>();

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
        StartCoroutine(Grab(iTransform));
    }

    IEnumerator Grab(ITransform iTransform) {
        while (IsGrabbed) {
            transform.position = iTransform.Position;
            transform.rotation = iTransform.Rotation;
            yield return null;
        }
    }

    public virtual void OnRelease(ITransform iTransform) {
        IsGrabbed = false;
    }

    void OnCollisionEnter(Collision other) {
        GrabDetector grabDetector = other.gameObject.GetComponentInParent<GrabDetector>();
        if (grabDetector != null) {
            if (!_grabDetectors.ContainsKey(grabDetector)) {
                _grabDetectors.Add(grabDetector, 0);
            }
            if (++_grabDetectors[grabDetector] == 1) {
                grabDetector.OnBegin += OnGrab;
                grabDetector.OnFinish += OnRelease;
            }
        }
    }

    void OnCollisionExit(Collision other) {
        GrabDetector grabDetector = other.gameObject.GetComponentInParent<GrabDetector>();
        if (grabDetector != null) {
            if (--_grabDetectors[grabDetector] == 0) {
                grabDetector.OnBegin -= OnGrab;
                grabDetector.OnFinish -= OnRelease;
            }
        }
    }
}
