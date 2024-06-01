using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Agent : MonoBehaviour {
    [SerializeField] float _duration = 1f;
    [SerializeField] AnimationCurve _speedCurve;

    enum AgentType { Blue, Yellow, Hitman, Target }

    enum Direction {
        Up, Left, Down, Right
    }
    static Direction Inverse(Direction d) {
        return (Direction) (((int) d + 2)%4);
    }

    Case fromDirection(Direction d) {
        switch (d) {
            case Direction.Up:
                return _case.Up;
            case Direction.Left:
                return _case.Left;
            case Direction.Down:
                return _case.Down;
            case Direction.Right:
                return _case.Right;
        }
        return null;
    }

    [SerializeField] AgentType _type;
    [SerializeField] Direction _direction;
    [SerializeField] Case _case;
    public Case Case { get { return _case; } set { _case = value; } }

    static readonly List<Agent> Agents = new List<Agent>();

    public static bool MoveAllAgents() {
        if (Agents.All(a => !a.Animated)) {
            Agents.ForEach(a => a.Next());
            return true;
        }
        return false;
    }

    int _animations;
    bool Animated { get { return _animations > 0; } }

    void Awake() {
        if (_type != AgentType.Hitman && _type != AgentType.Target)
            Agents.Add(this);
    }

    // TODO: move agents differently if there is other agents on the case (and move the agent on the case)
    // TODO: Kill hitman animation if on the next case
    // TODO: hitman kill agent
    // TODO: change opacity of pieces if they can't be held
    // TODO: reset game when hitman is thrown away of the board
    // TODO: physics animations
    // TODO: hitman win when kill the red agent
    // TODO: Move the board (position, rotation, scale?)
    // TODO: throw the board?
    void Next() {
        if (Animated) return; // animation security

        if (_type == AgentType.Blue) {
            _direction = Inverse(_direction);
            StartCoroutine(Rotate(Quaternion.AngleAxis(180f, Vector3.up)));
        } else if (_type == AgentType.Yellow) {
            _case = fromDirection(_direction);
            StartCoroutine(Move(_case.transform.position - transform.position, RotateOnRoadEnd()));
        }
    }

    IEnumerator RotateOnRoadEnd() {
        if (fromDirection(_direction) == null) {
            _direction = Inverse(_direction);
            yield return StartCoroutine(Rotate(Quaternion.AngleAxis(180f, Vector3.up)));
        }
    }

    IEnumerator Move(Vector3 v, IEnumerator next) {
        ++_animations;

        var startPos = transform.position;
        var endPos = transform.position + v;
        endPos.y = startPos.y;

        float timer = 0f;
        while (timer <= _duration) {
            transform.position = Vector3.Lerp(startPos, endPos, _speedCurve.Evaluate(timer/_duration));
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;

        if (next != null)
            yield return StartCoroutine(next);

        --_animations;
    }

    IEnumerator Rotate(Quaternion q) {
        ++_animations;

        var startRot = transform.rotation;
        var endRot = transform.rotation*q;

        float timer = 0f;
        while (timer <= _duration) {
            transform.rotation = Quaternion.Lerp(startRot, endRot, _speedCurve.Evaluate(timer/_duration));
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endRot;

        --_animations;
    }

    // TEST
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Next();
        }
    }
}
