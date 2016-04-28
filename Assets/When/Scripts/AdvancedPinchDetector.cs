using System;
using System.Collections;
using UnityEngine;
using Leap;
using When.Interfaces;

namespace When {

    /// <summary>
    /// An advanced utility class to aid in creating pinch based actions.
    /// Once linked with an IHandModel, it can be used to detect pinch gestures that the hand makes.
    /// 
    /// Gesture definition
    /// - Thumb pseudo-extended
    /// - Unique "movement" of index finger
    /// </summary>
    [RequireComponent(typeof(IHandModel))]
    public class AdvancedPinchDetector : MonoBehaviour, ITransform {
        public event Action<ITransform> OnBegin, OnFinish;

        const float MM_TO_M = 0.001f; // millimeters to meters
        
        [SerializeField] float _activatePinchDist = 0.02f;
        [SerializeField] float _desactivatePinchDist = 0.03f;
        [SerializeField] float _grabStrengthThreshold = 0.5f;
        [SerializeField] float _pinchStrengthThreshold = 0.5f;

        [SerializeField] AudioSource _beginAudioSource;

        IHandModel _handModel = null;
        Hand _hand = null;
        bool _isPinching = false;
        bool _isCancelled = false;
        
        float _pinchDistance = 0;
        float _grabStrength = 0;
        float _palmNormalDotCenterEye = 0;
        float _pinchStrength = 0;

        //float _lastPinchTime = 0.0f;
        //float _lastUnpinchTime = 0.0f;

        Vector3 _pinchPos, _lastPinchPos;
        Quaternion _pinchRotation;

        Transform centerEyeAnchor;

        IEnumerator _currentEnumerator;

        bool IsPinching {
            get { return _isPinching; }
            set {
                if (_isPinching != value) {
                    if (value) {
                        if (OnBegin != null) {
                            _beginAudioSource.pitch = UnityEngine.Random.Range(1f, 3f);
                            _beginAudioSource.Play();
                            OnBegin(this);
                        }
                    } else {
                        if (OnFinish != null) {
                            OnFinish(this);
                        }
                    }
                }
                _isPinching = value;
            }
        }

        // Activate distance cannot be greater than deactivate distance
        void OnValidate() {
            _activatePinchDist = Mathf.Max(_activatePinchDist, 0);
            _desactivatePinchDist = Mathf.Max(_desactivatePinchDist, 0);
            if (_activatePinchDist > _desactivatePinchDist) {
                _desactivatePinchDist = _activatePinchDist;
            }
        }

        void Awake() {
            _handModel = GetComponent<IHandModel>();
            centerEyeAnchor = GameObject.FindGameObjectWithTag("MainCamera").transform; // TODO serialized property ?
        }

        void OnEnable() {
            _currentEnumerator = Recognizer();
            StartCoroutine(_currentEnumerator);
        }

        //public float LastPinchTime { get { return _lastPinchTime; } }
        //public float LastUnpinchTime { get { return _lastUnpinchTime; } }
        public Vector3 Position { get { return _pinchPos; } }
        public Vector3 DeltaPosition { get { return _lastPinchPos /*- _pinchPos*/; } }
        public Quaternion Rotation { get { return _pinchRotation; } }
        public Vector3 Scale { get { return Vector3.one; } }

        void Update() {
            _hand = _handModel.GetLeapHand();
            if (_hand == null || !_handModel.IsTracked) {
                _isCancelled = true;
                return;
            }
            
            _palmNormalDotCenterEye = Vector3.Dot(_hand.PalmNormal.ToVector3(), centerEyeAnchor.forward);
            _pinchDistance = _hand.PinchDistance * MM_TO_M;
            _grabStrength = _hand.GrabStrength;
            _pinchStrength = _hand.PinchStrength;
            _pinchRotation = _hand.Basis.Rotation();
            _lastPinchPos = _hand.PalmVelocity.ToUnityScaled(); // _pinchPos;
            _pinchPos = Vector3.zero;
            for (int i = 0; i < _hand.Fingers.Count; i++) {
                Finger finger = _hand.Fingers[i];
                if (finger.Type == Finger.FingerType.TYPE_THUMB || finger.Type == Finger.FingerType.TYPE_INDEX) {
                    _pinchPos += finger.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3();
                }
            }
            _pinchPos /= 2.0f;
        }

        IEnumerator Recognizer() {
            try {
                while (true) {
                    do {
                        // Reset
                        _isCancelled = false;
                        yield return null;

                        // OutOfRange
                        while (_isCancelled || _pinchDistance < _desactivatePinchDist
                            /* || _palmNormalDotCenterEye < 0 */) {
                            _isCancelled = false;
                            yield return null;
                        }

                        // Possible
                        while (RequiredCond() && _pinchDistance > _activatePinchDist) {
                            yield return null;
                        }
                    } while (!RequiredCond()); // Fail

                    // Recognized !
                    IsPinching = true;
                    while (!_isCancelled && _pinchDistance < _desactivatePinchDist) {
                        yield return null;
                    }
                    IsPinching = false;
                }
            } finally {
                IsPinching = false;
            }
        }

        bool RequiredCond() {
            return !_isCancelled /* && _grabStrength < _grabStrengthThreshold && _palmNormalDotCenterEye > 0 */;
        }

        void OnDisable() {
            ((IDisposable) _currentEnumerator).Dispose();
            StopAllCoroutines(); // useless?
        }
    }
}