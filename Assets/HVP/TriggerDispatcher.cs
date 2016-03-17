using UnityEngine;
using System;

namespace HVP {
    public class TriggerDispatcher : MonoBehaviour {
        public event Action<Collider> OnEnterCollider, OnStayCollider, OnExitCollider;
        public event Action OnEnter, OnStay, OnExit;
        void Start() { }
        void OnTriggerEnter(Collider other) { if (OnEnterCollider != null) OnEnterCollider(other); if (OnEnter != null) OnEnter(); }
        void OnTriggerStay(Collider other) { if (OnStayCollider != null) OnStayCollider(other); if (OnStay != null) OnStay(); }
        void OnTriggerExit(Collider other) { if (OnExitCollider != null) OnExitCollider(other); if (OnExit != null) OnExit(); }
    }
}