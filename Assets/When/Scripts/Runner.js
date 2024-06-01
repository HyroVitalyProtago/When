// #pragma strict

var code;

function Start() {
    code = GetComponentInChildren.<UnityEngine.UI.Text>();
}

function Run() {
    eval(code.text);
}