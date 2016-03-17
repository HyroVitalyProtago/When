/******************************************************************************\
* Copyright (C) Ludovic David 2016.                                            *
* Licensed under MIT.                                                          *
* Available at https://opensource.org/licenses/MIT                             *
\******************************************************************************/

using System;
using UnityEngine;

namespace HVP {

	/// <summary>
	/// Connect events from the global system based on the EventConductor to a specific component.
	/// For each entry, if a callback name isn't specified, the default callback name correpond to the global event name
	/// TODO Support missing properties
	/// </summary>
	// [ExecuteAfter(typeof(GlobalEventSetter))]
	public class GlobalEventGetter : MonoBehaviour {

		[SerializeField] Entry[] connexions = null;

		void Start() {}

		void OnEnable() {
			foreach (var connexion in connexions) {
				EventConductor.On(connexion.receiver, connexion.globalEventName, CallbackName(connexion));
			}
		}

		void OnDisable() {
			foreach (var connexion in connexions) {
				EventConductor.Off(connexion.receiver, connexion.globalEventName);
			}
		}

		string CallbackName(Entry e) {
			return string.IsNullOrEmpty(e.callbackName) ? e.globalEventName : e.callbackName;
		}

		[Serializable]
		class Entry {
			public string globalEventName = null;
			public Component receiver = null;
			public string callbackName = null;
			public Entry() { }
		}
	}
}