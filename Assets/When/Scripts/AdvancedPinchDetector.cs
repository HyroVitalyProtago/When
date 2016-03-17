using System;
using System.Collections;
using UnityEngine;
using Leap;
using When.Interfaces;

namespace When {

    /// <summary>
    /// A basic utility class to aid in creating pinch based actions.
    /// Once linked with an IHandModel, it can be used to detect pinch gestures that the hand makes.
    /// </summary>
    [RequireComponent(typeof(IHandModel))]
    public class AdvancedPinchDetector : MonoBehaviour, ITransform {
        public event Action<ITransform> OnBegin, OnFinish;

        const float MM_TO_M = 0.001f;

        [SerializeField] float deltaThershold;

        [SerializeField]
        [Range(0, Mathf.Infinity)]
        float _activatePinchDist = 0.03f;
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        float _desactivatePinchDist = 0.04f;

        IHandModel _handModel = null;
        Hand _hand = null;
        //bool _isPinching = false;
        bool _isCancelled = false;
        Vector3 _lastIndexPos, _lastThumbPos;
        float _pinchDistance = 0;
        bool _indexDeltaToThumb = false;
        bool _thumbExtended = false;

        //float _lastPinchTime = 0.0f;
        //float _lastUnpinchTime = 0.0f;

        Vector3 _pinchPos;
        Quaternion _pinchRotation;

        // Activate distance cannot be greater than deactivate distance
        void OnValidate() {
            if (_activatePinchDist > _desactivatePinchDist) {
                _desactivatePinchDist = _activatePinchDist;
            }
        }

        void Awake() {
            _handModel = GetComponent<IHandModel>();
        }

        void OnEnable() {
            StartCoroutine(Recognizer());
        }

        //public float LastPinchTime { get { return _lastPinchTime; } }
        //public float LastUnpinchTime { get { return _lastUnpinchTime; } }
        public Vector3 Position { get { return _pinchPos; } }
        public Quaternion Rotation { get { return _pinchRotation; } }
        public Vector3 Scale { get { return Vector3.one; } }

        void Update() {
            _hand = _handModel.GetLeapHand();
            if (_hand == null || !_handModel.IsTracked) {
                _isCancelled = true;
                return;
            }

            _pinchDistance = _hand.PinchDistance * MM_TO_M;
            _pinchRotation = _hand.Basis.Rotation();

            Vector3 deltaIndex = Vector3.zero, deltaThumb = Vector3.zero;

            var fingers = _hand.Fingers;
            _pinchPos = Vector3.zero;
            for (int i = 0; i < fingers.Count; i++) {
                Finger finger = fingers[i];
                if (finger.Type == Finger.FingerType.TYPE_INDEX || finger.Type == Finger.FingerType.TYPE_THUMB) {
                    Vector3 pos = finger.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3();

                    _pinchPos += pos;

                    if (finger.Type == Finger.FingerType.TYPE_INDEX) {
                        deltaIndex = pos - _lastIndexPos;
                        _lastIndexPos = pos;
                    } else {
                        _thumbExtended = finger.IsExtended;
                        deltaThumb = pos - _lastThumbPos;
                        _lastThumbPos = pos;
                    }
                }
            }
            _pinchPos /= 2.0f;

            _indexDeltaToThumb = Vector3.Angle(deltaIndex, _lastThumbPos - _lastIndexPos) < deltaThershold;
        }

        IEnumerator Recognizer() {
            _isCancelled = false;
            yield return null;

            // OutOfRange
            while (_pinchDistance < _desactivatePinchDist) { _isCancelled = false; yield return null; }
            
            // Possible
            while (!_isCancelled && _indexDeltaToThumb && _thumbExtended && _pinchDistance > _activatePinchDist) { yield return null; }

            // Fail
            if (_isCancelled || !_indexDeltaToThumb || !_thumbExtended) { StartCoroutine(Recognizer()); yield break; }

            // Recognized !
            if (OnBegin != null) OnBegin(this);
            while (!_isCancelled && _pinchDistance < _desactivatePinchDist) { yield return null; }
            if (OnFinish != null) OnFinish(this);

            StartCoroutine(Recognizer());
        }

        void OnDisable() {
            _isCancelled = true;
        }

        void OnDestroy() {
            _isCancelled = true;
        }
    }
}