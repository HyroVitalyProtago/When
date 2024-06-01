using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using When;

public class Case : MonoBehaviour {
    [SerializeField] Case _up, _down, _left, _right;

    [SerializeField] float _duration = 1f;
    [SerializeField] AnimationCurve _speedCurve = AnimationCurve.EaseInOut(0,0,1,1);

    public Case Up { get { return _up; } }
    public Case Down { get { return _down; } }
    public Case Left { get { return _left; } }
    public Case Right { get { return _right; } }

    public bool IsNextTo(Case c) {
        return Up == c || Down == c || Left == c || Right == c;
    }

    Renderer _renderer;
    Color _origColor;

    void Awake() {
        _renderer = GetComponent<Renderer>();
        _origColor = _renderer.material.color;
    }
    
    IEnumerator ChangeColor(Color c, IEnumerator next = null) {
        var startColor = _renderer.material.color;
        var endColor = c;

        float timer = 0f;
        while (timer <= _duration) {
            _renderer.material.color = Color.Lerp(startColor, endColor, _speedCurve.Evaluate(timer / _duration));
            timer += Time.deltaTime;
            yield return null;
        }
        _renderer.material.color = endColor;

        if (next != null)
            yield return StartCoroutine(next);
    }

    readonly Dictionary<PinchableObject, Action<bool>> _objs = new Dictionary<PinchableObject, Action<bool>>();

    // TODO check if the player isn't held
    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Player")) {

            var agent = collision.gameObject.GetComponent<Agent>();
            Action<bool> action = (b) => {
                if (b) return;

                if (agent.Case == this) { // no moves
                    StartCoroutine(ChangeColor(Color.white, ChangeColor(_origColor)));
                } else if (!IsNextTo(agent.Case)) { // invalid move
                    StartCoroutine(ChangeColor(Color.red, ChangeColor(_origColor)));
                    agent.Case.StartCoroutine(agent.Case.ChangeColor(Color.blue, agent.Case.ChangeColor(agent.Case._origColor)));
                } else {
                    collision.gameObject.GetComponent<Agent>().Case = this;
                    if (!Agent.MoveAllAgents()) { // TODO check if it works, else : wait the end of animations
                        StartCoroutine(WaitEndOfAnimation());
                    } else {
                        StartCoroutine(ChangeColor(Color.green, ChangeColor(_origColor)));
                    }
                }
            };

            var obj = agent.GetComponent<PinchableObject>();
            if (obj.IsHeld) {
                obj.OnHeldChange += action;
                _objs.Add(obj, action);
                return;
            }

            action(false);
        }
    }

    IEnumerator WaitEndOfAnimation() {
        while (!Agent.MoveAllAgents()) {
            yield return StartCoroutine(ChangeColor(Color.green, ChangeColor(_origColor)));
        }
    }

    void OnCollisionExit(Collision collision) {
        var obj = collision.gameObject.GetComponent<PinchableObject>();
        if (obj != null && _objs.ContainsKey(obj)) {
            obj.OnHeldChange -= _objs[obj];
            _objs.Remove(obj);
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        if (Up != null) Gizmos.DrawSphere(Up.transform.position + Vector3.up*.1f, .01f);

        Gizmos.color = Color.red;
        if (Down != null) Gizmos.DrawSphere(Down.transform.position + Vector3.up*.1f, .01f);

        Gizmos.color = Color.yellow;
        if (Left != null) Gizmos.DrawSphere(Left.transform.position + Vector3.up*.1f, .01f);

        Gizmos.color = Color.green;
        if (Right != null) Gizmos.DrawSphere(Right.transform.position + Vector3.up*.1f, .01f);
    }
}
