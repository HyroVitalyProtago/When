using System;
using UnityEngine;
using When.Interfaces;

namespace When {
    public class DoublePinchRecognizer : MonoBehaviour {
        public event Action<ITransform, ITransform> OnBegin, OnFinish;

        ITransform _pinch1 = null, _pinch2 = null;
        bool _isRecognized = false;

        void Start() { }

        public void OnPinchBegin(ITransform iPosition) {
            if (_pinch1 == null) {
                _pinch1 = iPosition;
            } else if (_pinch2 == null) {
                _pinch2 = iPosition;
            }

            if (!_isRecognized && _pinch1 != null && _pinch2 != null) {
                _isRecognized = true;
                if (OnBegin != null) OnBegin(_pinch1, _pinch2);
            }
        }

        public void OnPinchFinish(ITransform iPosition) {
            if (_pinch1 == iPosition || _pinch2 == iPosition) {

                if (_isRecognized) {
                    _isRecognized = false;
                    if (OnFinish != null) OnFinish(_pinch1, _pinch2);
                }

                if (_pinch1 == iPosition) {
                    _pinch1 = null;
                } else {
                    _pinch2 = null;
                }
            }
        }
    }
}
