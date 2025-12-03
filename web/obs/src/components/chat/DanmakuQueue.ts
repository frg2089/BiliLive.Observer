import * as types from './ChatMessageType'

export class DanmakuQueue {
  messages = reactive<types.AnyDisplayMessage[]>([])

  private static nextId = (): string => crypto.randomUUID()

  dispatch(event: Bili.Live.Danmaku.Event) {
    if (!event) return

    switch (event.type) {
      case 'Comment':
        enqueue({
          type: types.MessageType.TEXT,
          id: nextId(),
          avatarUrl: event.userAvatar,
          time: new Date(),
          authorName: event.userName,
          uid: `${event.userID}`,
          privilegeType: event.userGuardLevel,

          medalLevel: 0,
          medalName: '',
        })
        break
      case 'GiftSend':
        enqueue({
          type: types.MessageType.GIFT,
          id: nextId(),
        })
        break
    }
  }

  private enqueue(message: types.AnyDisplayMessage) {}
}
