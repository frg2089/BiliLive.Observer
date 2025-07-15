namespace BiliLive.Service.Model;

public sealed record class StartLiveRequest(int RoomId, int AreaId);
public sealed record class SendChatRequest(int RoomId, string Message);
public sealed record class ChatEventRequest(int RoomId, long UserId);
public sealed record class UpdateLiveRequest(int RoomId, string? Platform = default, string? VisitId = default, string? Title = default, int? AreaId = default, string? AddTag = default, string? DelTag = default);