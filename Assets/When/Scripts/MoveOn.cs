using System;
using System.Collections;
using UnityEngine;

public class MoveOn : MonoBehaviour {

    public event Action OnStartArrive;
    public event Action OnEndArrive;

    [SerializeField] float duration;
    [SerializeField] Vector3 delta;
    [SerializeField] bool onRigidbody;

    Vector3 startPos, endPos;
    bool activated;
    new Rigidbody rigidbody;

    void Awake() {
        startPos = transform.position;
        endPos = startPos + transform.TransformDirection(delta);
        activated = false;
        if (onRigidbody) rigidbody = GetComponent<Rigidbody>();
    }

    public void Move(bool b) {
        if (activated == b) return;
        StopAllCoroutines();
        StartCoroutine(MoveCoroutine(b));
    }

    IEnumerator MoveCoroutine(bool b) {
        activated = b;

        Vector3 startPos = transform.position;
        Vector3 endPos = !activated ? this.startPos : this.endPos;

        float timer = 0f;
        while (timer <= duration) {
            if (onRigidbody) {
                rigidbody.MovePosition(Vector3.Lerp(startPos, endPos, timer / duration));
            } else {
                transform.position = Vector3.Lerp(startPos, endPos, timer / duration);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos; // set the position to the better approximation

        if (endPos == this.startPos && OnStartArrive != null) {
            OnStartArrive();
        } else if (endPos == this.endPos && OnEndArrive != null) {
            OnEndArrive();
        }
    }
}
