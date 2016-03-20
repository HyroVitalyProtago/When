using System;
using UnityEngine;
using Leap;
using When.Interfaces;

namespace When {

    public class GrabDetector : MonoBehaviour, ITransform {
        public event Action<ITransform> OnBegin, OnFinish;

        [SerializeField] IHandModel _handModel;
        [SerializeField] [Range(0, Mathf.Infinity)] float _activateGrabThreshold = 0.5f;
        [SerializeField] [Range(0, Mathf.Infinity)] float _deactivateGrabThreshold = 0.4f;
        
        bool _isGrabbing = false;

        Vector3 _grabPos;
        Quaternion _grabRotation;

        public Vector3 Position { get { return _grabPos; } }
        public Quaternion Rotation { get { return _grabRotation; } }
        public Vector3 Scale { get { return Vector3.one; } }

        // TODO
        void OnValidate() { }

        void Awake() {
            if (_handModel == null) {
                Debug.LogWarning("IHandModel required.");
                enabled = false;
            }
        }

        void Start() {}

        void Update() {
            Hand hand = _handModel.GetLeapHand();
            if (hand == null || !_handModel.IsTracked) {
                ChangeGrabState(false);
                return;
            }

            if (_isGrabbing) {
                if (hand.GrabStrength < _deactivateGrabThreshold) {
                    ChangeGrabState(false);
                    return;
                }
            } else {
                if (hand.GrabStrength > _activateGrabThreshold) {
                    ChangeGrabState(true);
                    return;
                }
            }
            
            _grabPos = hand.PalmPosition.ToUnityScaled();
            //_grabRotation = hand.;
        }

        void ChangeGrabState(bool shouldBeGrabbing) {
            if (_isGrabbing != shouldBeGrabbing) {
                _isGrabbing = shouldBeGrabbing;

                if (_isGrabbing) {
                    if (OnBegin != null) OnBegin(this);
                } else {
                    if (OnFinish != null) OnFinish(this);
                }
            }
        }

        void OnDisable() {
            StopAllCoroutines();
            ChangeGrabState(false);
        }

        void OnDestroy() {
            StopAllCoroutines();
            ChangeGrabState(false);
        }
    }
}