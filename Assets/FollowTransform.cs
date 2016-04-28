using UnityEngine;
using When.Interfaces;

[RequireComponent(typeof(Renderer), typeof(ITransform))]
public class FollowTransform : MonoBehaviour {

    [SerializeField] GameObject _gameObject;
    ITransform _iTransform;
    Renderer _renderer;

    void Awake() {
        _iTransform = _gameObject.GetComponent<ITransform>();
        _renderer = GetComponent<Renderer>();
    }

    public void OnBegin() {
        _renderer.enabled = true;
    }

    public void OnFinish() {
        _renderer.enabled = false;
    }

	void Update() {
	    transform.position = _iTransform.Position;
        transform.rotation = _iTransform.Rotation;
    }
}
