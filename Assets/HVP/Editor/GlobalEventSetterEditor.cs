using UnityEditor;
using UnityEngine;

namespace HVP.Editor {

	[CustomEditor(typeof(GlobalEventSetter), true)]
	public class GlobalEventSetterEditor : UnityEditor.Editor {

		static readonly GUIContent m_IconToolbarPlus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Plus"));
		static readonly GUIContent m_IconToolbarMinus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"));
		static readonly Texture2D gray = HVP.Editor.EditorUtility.GetTexture2DFromColor(new Color(.89f, .89f, .89f));

		SerializedProperty p_connexions;

		GUIStyle _boxStyle;

		GUIStyle boxStyle {
			get {
				if (_boxStyle == null) {
					_boxStyle = new GUIStyle(GUI.skin.box);
					_boxStyle.normal.background = gray;
				}
				return _boxStyle;
			}
		}

		GenericMenu eventMenu;

		void OnEnable() {
			p_connexions = serializedObject.FindProperty("connexions");
			eventMenu = new GenericMenu();
			eventMenu.AddDisabledItem(new GUIContent("No Event"));
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			DrawConnexionSystem(); // <sender> offer <senderEventName> under the name <globalEventName>
			serializedObject.ApplyModifiedProperties();
		}

		void DrawConnexionSystem() {
			EditorGUILayout.Space();

			Rect lastRect;
			int toBeRemovedEntry = -1;
			Vector2 removeButtonSize = GUIStyle.none.CalcSize(m_IconToolbarMinus);
			Vector2 plusButtonSize = GUIStyle.none.CalcSize(m_IconToolbarPlus);

			for (int i = 0; i < p_connexions.arraySize; ++i) {
				SerializedProperty delegateProperty = p_connexions.GetArrayElementAtIndex(i);
				SerializedProperty sender = delegateProperty.FindPropertyRelative("sender");
				SerializedProperty eventName = delegateProperty.FindPropertyRelative("senderEventName");
				SerializedProperty globalEventName = delegateProperty.FindPropertyRelative("globalEventName");

				EditorGUILayout.BeginVertical(boxStyle);

				EditorGUILayout.Space();
				EditorGUILayout.Space(); // just because it's more comfortable
				lastRect = GUILayoutUtility.GetLastRect();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(sender, GUIContent.none);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				string[] eventOptions = EventConductor.GetEventsOf(sender.objectReferenceValue as Component);
				EditorGUI.BeginDisabledGroup(eventOptions.Length == 1); // or sender == null
				HVP.Editor.EditorUtility.Popup(eventName, ref eventOptions);
				EditorGUI.EndDisabledGroup();
				EditorGUILayout.PropertyField(globalEventName, GUIContent.none);
				EditorGUILayout.EndHorizontal();

				Rect removeButtonPos = new Rect(
					lastRect.xMax - removeButtonSize.x - 4,
					lastRect.y - 8,
					removeButtonSize.x,
					removeButtonSize.y
				);
				if (GUI.Button(removeButtonPos, m_IconToolbarMinus, GUIStyle.none)) {
					toBeRemovedEntry = i;
				}

				EditorGUILayout.EndVertical();
			}

			if (toBeRemovedEntry > -1) {
				p_connexions.DeleteArrayElementAtIndex(toBeRemovedEntry);
			}

			EditorGUILayout.BeginHorizontal();
			Rect rect = EditorGUILayout.GetControlRect();
			Rect plusButtonPos = new Rect(
				rect.xMax - plusButtonSize.x - 8,
				rect.y - 1,
				plusButtonSize.x,
				plusButtonSize.y
			);
			if (GUI.Button(plusButtonPos, m_IconToolbarPlus, GUIStyle.none)) {
				p_connexions.arraySize++;
				SerializedProperty delegateProperty = p_connexions.GetArrayElementAtIndex(p_connexions.arraySize - 1);
				delegateProperty.FindPropertyRelative("sender").objectReferenceValue = null;
				delegateProperty.FindPropertyRelative("senderEventName").stringValue = string.Empty;
				delegateProperty.FindPropertyRelative("globalEventName").stringValue = string.Empty;
				serializedObject.ApplyModifiedProperties();
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}