using UnityEngine;
using System;

public class VRButton : MonoBehaviour {
    [SerializeField] Color _onColor = Color.green, _offColor = Color.blue;
    [SerializeField] float _activationThreshold = -.1f;

    Renderer _renderer;
    bool _toInit, _activated;

    public event Action<bool> Activate;

    public bool Activated {
        get { return _activated; }
        set {
            if (value != _activated) {
                _activated = value;
                if (_activated) {
                    _renderer.material.color = _onColor;
                } else {
                    _renderer.material.color = _offColor;
                }
                if (Activate != null) Activate(_activated);
            }
        }
    }

    void Awake() {
        _renderer = GetComponent<Renderer>();
    }

    void Update() {
        if (_toInit && transform.localPosition.x < _activationThreshold) {
            Activated = !Activated;
            _toInit = false;
        } else if (Mathf.Abs(transform.localPosition.x) < .00001f) {
            _toInit = true;
        }
    }
}
