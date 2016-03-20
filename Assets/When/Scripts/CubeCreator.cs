using System.Collections;
using UnityEngine;
using When.Interfaces;

namespace When {

    // TODO don't collide with hands on release
    public class CubeCreator : MonoBehaviour {
        [SerializeField] GameObject primitive;
        [SerializeField] float maxSize = 1;
        [SerializeField] float scaleFactor = .5f;

        ITransform _t1, _t2;
        GameObject _cube;

        void Start() { }

        public void OnBegin(ITransform t1, ITransform t2) {
            _t1 = t1;
            _t2 = t2;
            StartCoroutine(Create());
        }

        public void OnFinish(ITransform t1, ITransform t2) {
            if (_t1 == t1 && _t2 == t2) {
                Stop();
                _t1 = _t2 = null;
            }
        }

        void Stop() {
            StopAllCoroutines();
            //_cube.AddComponent<Rigidbody>();
        }

        IEnumerator Create() {
            _cube = Instantiate(primitive);
            while (true) {
                _cube.transform.position = (_t1.Position + _t2.Position) * .5f;
                _cube.transform.rotation = Quaternion.Lerp(_t1.Rotation, _t2.Rotation, .5f);
                _cube.transform.localScale = Vector3.one * Mathf.Min(Vector3.Distance(_t1.Position, _t2.Position) * scaleFactor, maxSize);
                yield return null;
            }
        }
    }
}