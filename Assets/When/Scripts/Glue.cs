using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Glue : MonoBehaviour {
    Collider _collider ;

    void Awake() {
        _collider = GetComponent<Collider>();
    }

    void Start() {
        transform.GetComponentsInChildren<Collider>().ToList().ForEach(c => Physics.IgnoreCollision(_collider, c));
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.rigidbody != null && !collision.gameObject.name.Contains("Dart")) { // TODO case of another dart
            gameObject.AddComponent<CustomFixedJoint>().Apply(collision.rigidbody, collision.collider);
        }
    }
}
