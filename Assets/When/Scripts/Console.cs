using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Console : MonoBehaviour {
    public UnityEvent Run;

    Text _text;
    int _cursor = 0;

    void Awake() {
        _text = GetComponentInChildren<Text>();
    }

    void Update() {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Return)) {
            Run.Invoke();
        } else if(Input.GetKeyDown(KeyCode.Backspace) && _text.text.Length > 0 && _cursor > 0) {
            _text.text = _text.text.Substring(0, _cursor - 1) + _text.text.Substring(_cursor);
            _cursor--;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow) && _cursor > 0) {
            _cursor--;
        } else if (Input.GetKeyDown(KeyCode.RightArrow) && _cursor < _text.text.Length) {
            _cursor++;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            var idx = _text.text.IndexOf("\n", _cursor);
            if (idx == -1) return;
            _cursor = idx + 1;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            var idx = _text.text.IndexOf("\n");
            if (idx == -1 || idx > _cursor) return;
            _cursor = idx + 1;
        } else if (Input.inputString.Length != 0) {
            _text.text = _text.text.Substring(0, _cursor) + Input.inputString + _text.text.Substring(_cursor);
            _cursor++;
        }
    }
}
