using System;
using UnityEngine;

public class IntComponent : MonoBehaviour {
    public event Action<int> OnValueChange;

    int _value;
    public int Value {
        get { return _value; }
        set {
            if (_value != value) {
                _value = value;
                if (OnValueChange != null)
                    OnValueChange(_value);
            }
        }
    }
    
    void Start() {
        Value = 1;
    }

    public void Add(int i) {
        Value += i;
    }

    public void Mul(int i) {
        Value *= i;
    }

    public void Div(int i) {
        Value /= i;
    }
}
