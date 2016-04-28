using UnityEngine;
using System;

namespace HVP {
    public class TriggerDispatcher : MonoBehaviour {
        public event Action<Collider> OnEnterCollider, OnStayCollider, OnExitCollider;
        void Start() { }
        void OnTriggerEnter(Collider other) { if (OnEnterCollider != null) OnEnterCollider(other); }
        void OnTriggerStay(Collider other) { if (OnStayCollider != null) OnStayCollider(other); }
        void OnTriggerExit(Collider other) { if (OnExitCollider != null) OnExitCollider(other); }
    }
}