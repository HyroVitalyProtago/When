using System;
using UnityEngine;
using Leap;
using When.Interfaces;

namespace When {

    /// <summary>
    /// A basic utility class to aid in creating pinch based actions.
    /// Once linked with an IHandModel, it can be used to detect pinch gestures that the hand makes.
    /// </summary>
    [RequireComponent(typeof(IHandModel))]
    public class PinchDetector : MonoBehaviour, ITransform {
        public event Action<ITransform> OnBegin, OnFinish;

        const float MM_TO_M = 0.001f;

        [SerializeField]
        [Range(0, Mathf.Infinity)]
        float _activatePinchDist = 0.03f;
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        float _desactivatePinchDist = 0.04f;

        IHandModel _handModel = null;
        bool _isPinching = false;

        float _lastPinchTime = 0.0f;
        float _lastUnpinchTime = 0.0f;

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

        public float LastPinchTime { get { return _lastPinchTime; } }
        public float LastUnpinchTime { get { return _lastUnpinchTime; } }
        public Vector3 Position { get { return _pinchPos; } }
        public Vector3 DeltaPosition { get { return _pinchPos; } }
        public Quaternion Rotation { get { return _pinchRotation; } }
        public Vector3 Scale { get { return Vector3.one; } }

        void Update() {
            Hand hand = _handModel.GetLeapHand();
            if (hand == null || !_handModel.IsTracked) {
                ChangePinchState(false);
                return;
            }

            float pinchDistance = hand.PinchDistance * MM_TO_M;
            _pinchRotation = hand.Basis.Rotation();

            var fingers = hand.Fingers;
            _pinchPos = Vector3.zero;
            for (int i = 0; i < fingers.Count; i++) {
                Finger finger = fingers[i];
                if (finger.Type == Finger.FingerType.TYPE_INDEX ||
                    finger.Type == Finger.FingerType.TYPE_THUMB) {
                    _pinchPos += finger.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3();
                } else if (!finger.IsExtended) { // other fingers need to be extended
                    ChangePinchState(false);
                    return;
                }
            }
            _pinchPos /= 2.0f;

            if (_isPinching) {
                if (pinchDistance > _desactivatePinchDist) {
                    ChangePinchState(false);
                    return;
                }
            } else {
                if (pinchDistance < _activatePinchDist) {
                    ChangePinchState(true);
                }
            }

            //if (_isPinching) {
            //    _pinchPos = transform.position;
            //    _pinchRotation = transform.rotation;
            //}
        }

        void ChangePinchState(bool shouldBePinching) {
            if (_isPinching != shouldBePinching) {
                _isPinching = shouldBePinching;

                if (_isPinching) {
                    _lastPinchTime = Time.time;
                    if (OnBegin != null) OnBegin(this);
                } else {
                    _lastUnpinchTime = Time.time;
                    if (OnFinish != null) OnFinish(this);
                }
            }
        }

        void OnDisable() {
            ChangePinchState(false);
        }

        void OnDestroy() {
            ChangePinchState(false);
        }
    }
}