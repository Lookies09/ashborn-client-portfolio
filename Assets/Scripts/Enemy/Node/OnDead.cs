using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/OnDead")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "OnDead", message: "DeadEvent", category: "Events", id: "a1034022e317b7bfa3c5faa57342e8c1")]
public sealed partial class OnDead : EventChannel { }

