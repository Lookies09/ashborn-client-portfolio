using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/OnStateChanged")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "OnStateChanged", message: "StateChanged", category: "Events", id: "78ae4f8a39c8a0916ea241e2ee19bb33")]
public sealed partial class OnStateChanged : EventChannel<EnemyState> { }

