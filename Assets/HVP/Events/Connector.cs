/******************************************************************************\
* Copyright (C) Ludovic David 2016.                                            *
* Licensed under MIT.                                                          *
* Available at https://opensource.org/licenses/MIT                             *
\******************************************************************************/

using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace HVP {

	public class Connector : MonoBehaviour {

		[SerializeField] Entry[] connexions = null;

		void Awake() {
			foreach (Entry entry in connexions) {
				if (entry.sender == null || entry.receiver == null || string.IsNullOrEmpty(entry.senderEventName) ||
					string.IsNullOrEmpty(entry.receiverCallbackName)) {
					Debug.LogWarning("Invalid entry in a connector...", this);
					continue;
				}

				entry.eventAdd = entry.sender.GetType().GetEvent(entry.senderEventName).GetAddMethod(false);
				entry.eventRem = entry.sender.GetType().GetEvent(entry.senderEventName).GetRemoveMethod(false);
				if (entry.eventAdd == null || entry.eventRem == null) {
					throw new EventConductor.EventNotFoundException();
				}

				Type[] types = entry.sender.GetType().GetEvent(entry.senderEventName).EventHandlerType.GetMethod("Invoke").GetParameters().Select(p => p.ParameterType).ToArray();
				MethodInfo method = entry.receiver.GetType().GetMethod(entry.receiverCallbackName, EventConductor.InstancePublic, null, types, null);
				if (method == null) {
                    method = entry.receiver.GetType().GetMethods(EventConductor.InstancePublic).First(m => m.Name == entry.receiverCallbackName); // Get a random one with the name...
                    if (method == null) {
				        throw new EventConductor.CallbackNotFoundException();
				    }
                    Debug.LogFormat(this, "[Connector] Random callback {0} get in receiver {1} to resolve ambiguity.", entry.receiverCallbackName, entry.receiver);
                }

				try {
					entry.callback = Delegate.CreateDelegate(EventConductor.DelegateType(method), entry.receiver, entry.receiverCallbackName);
					entry.enabled = true;
				} catch (ArgumentException) {
					throw new EventConductor.CallbackBadTypeException();
				} // MethodAccessException
			}
		}

		void Start() {} // can be disabled in editor

		void OnEnable() {
			foreach (Entry entry in connexions) {
				if (entry.enabled) Connect(entry, true);
			}
		}

		void OnDisable() {
			foreach (Entry entry in connexions) {
				if (entry.enabled) Connect(entry, false);
			}
		}

		void Connect(Entry entry, bool value) {
			try {
				(value ? entry.eventAdd : entry.eventRem).Invoke(entry.sender, new object[] { entry.callback });
			} catch (ArgumentException) {
			    try {
			        ParameterExpression[] parametersExpressions = entry.sender
			            .GetType()
			            .GetEvent(entry.senderEventName)
			            .EventHandlerType
			            .GetMethod("Invoke")
			            .GetParameters()
			            .Select(p => Expression.Parameter(p.ParameterType, Path.GetRandomFileName().Replace(".", "")))
			            .ToArray();

			        Delegate realCallback = entry.callback;
			        object[] args = realCallback.Method.GetParameters().Select(parameter => {
                        Type t = parameter.ParameterType;
			            return t.IsValueType ? Activator.CreateInstance(t) : null;
			        }).ToArray();
                    Action func = delegate { realCallback.DynamicInvoke(args); };
                    var body = Expression.Call(Expression.Constant(func.Target), func.Method);
                    entry.callback = Expression.Lambda(body, parametersExpressions).Compile(); // override callback for on/off event subcribing
                    (value ? entry.eventAdd : entry.eventRem).Invoke(entry.sender, new object[] { entry.callback });
                    Debug.LogFormat(this, "[Connector] Adapter constructed between [{0}.{1}] += [{2}.{3}]", entry.sender, entry.senderEventName, entry.receiver, entry.receiverCallbackName);
                } catch (ArgumentException) {
                    throw new EventConductor.EventNotMatchCallbackException();
                }
			}
		}

		[Serializable]
		class Entry {
			public Component sender = null, receiver = null;
			public string senderEventName = null, receiverCallbackName = null;
			public Entry() {}
			[NonSerialized] public bool enabled; // not used in editor
			[NonSerialized] public MethodInfo eventAdd, eventRem; // not used in editor
			[NonSerialized] public Delegate callback; // not used in editor
		}
	}
}