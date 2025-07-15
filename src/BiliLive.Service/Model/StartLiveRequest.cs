namespace BiliLive.Service.Model;

public sealed record class StartLiveRequest(int RoomId, int AreaId);
public sealed record class SendChatRequest(int RoomId, string Message);
public sealed record class ChatEventRequest(int RoomId, long UserId);