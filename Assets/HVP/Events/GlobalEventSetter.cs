/******************************************************************************\
* Copyright (C) Ludovic David 2016.                                            *
* Licensed under MIT.                                                          *
* Available at https://opensource.org/licenses/MIT                             *
\******************************************************************************/

using System;
using System.Linq;
using UnityEngine;

namespace HVP {

	/// <summary>
	/// Connect events to the global system based on the EventConductor.
	/// For each entry, if a global event name isn't specified, the default global event name correpond to the event name
	/// TODO Support missing properties
	/// </summary>
	public class GlobalEventSetter : MonoBehaviour {

		[SerializeField] Entry[] connexions = null;

		void Start() {}

		void OnEnable() {
			foreach (var connexion in connexions) {
				EventConductor.Offer(connexion.sender, connexion.senderEventName, GlobalEventName(connexion));
			}
		}

		void OnDisable() {
			foreach (var connexion in connexions) {
				EventConductor.Denial(connexion.sender, GlobalEventName(connexion));
			}
		}

		string GlobalEventName(Entry e) {
			return string.IsNullOrEmpty(e.globalEventName) ? e.senderEventName : e.globalEventName;
		}

		public string[] GetAllGlobalEventName() {
			return connexions.Select(c => GlobalEventName(c)).ToArray();
		}

		[Serializable]
		class Entry {
			public Component sender = null;
			public string senderEventName = null;
			public string globalEventName = null;
			public Entry() { }
		}
	}
}