using UnityEngine;
using System;

public class OnTrigger : MonoBehaviour {
    public event Action<int> OnEnter;

    [SerializeField] Collider collider;
    [SerializeField] int value;

    void OnTriggerEnter(Collider other) {
        if (other != collider) return;
        if (OnEnter != null) OnEnter(value);
    }
}
