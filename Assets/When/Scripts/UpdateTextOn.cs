using UnityEngine;
using UnityEngine.UI;

namespace CHSF {
	[RequireComponent(typeof(Text))]
	public class UpdateTextOn : MonoBehaviour {
		Text _text;

		void Awake() {
			_text = GetComponent<Text>();
		}

		public void UpdateString(string s) {
			_text.text = s;
		}

		public void UpdateInt(int i) {
			_text.text = i.ToString();
		}

		public void UpdateFloat(float f) {
			_text.text = f.ToString("0.00");
		}
	}
}
