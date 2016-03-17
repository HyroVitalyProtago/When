using UnityEngine;

public class Switch : MonoBehaviour {

    [SerializeField] Behaviour behaviour;
    [SerializeField] bool disableOnStart;

    void Start() {
        if (disableOnStart)
            Off();
    }

    public void Toggle() {
        behaviour.enabled = !behaviour.enabled;
    }

    public void On() {
        behaviour.enabled = true;
    }

    public void Off() {
        behaviour.enabled = false;
    }

    public void Set(bool b) {
        behaviour.enabled = b;
    }

    public void Not(bool b) {
        behaviour.enabled = !b;
    }
}
