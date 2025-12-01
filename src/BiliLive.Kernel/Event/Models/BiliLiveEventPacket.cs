using System.Text.Json;
using System.Text.Json.Serialization;

namespace BiliLive.Kernel.Event.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(BiliLiveHotEventPacket), "hot")]
[JsonDerivedType(typeof(BiliLiveNotificationEventPacket), "notification")]
public abstract record class BiliLiveEventPacket();
public sealed record class BiliLiveHotEventPacket(int Hot) : BiliLiveEventPacket;
public sealed record class BiliLiveNotificationEventPacket(JsonElement JsonElement) : BiliLiveEventPacket;