using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/OnDamage")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "OnDamage", message: "[Self] Take Dmg", category: "Events", id: "a0cd6a57a8a5df98fbac7b57eee5d4e7")]
public sealed partial class OnDamage : EventChannel<GameObject> { }

