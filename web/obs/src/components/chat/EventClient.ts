import { parseServerSentEvents } from 'parse-sse'

import { client } from '../../api'
import * as types from './ChatMessageType'

interface BiliEventMap {
  hot: number
  viewed: number
  message: types.AnyDisplayMessage
}

export class EventClient {
  private _eventSource = new EventTarget()

  constructor(
    readonly roomId: number,
    readonly userId: number,
  ) {}
  addEventListener<EventType extends keyof BiliEventMap>(
    type: EventType,
    callback: (data: CustomEvent<BiliEventMap[EventType]>) => void,
  ): void {
    this._eventSource.addEventListener(type, callback as EventListener)
  }
  removeEventListener<EventType extends keyof BiliEventMap>(
    type: EventType,
    callback: (data: CustomEvent<BiliEventMap[EventType]>) => void,
  ): void {
    this._eventSource.removeEventListener(type, callback as EventListener)
  }

  public start = async () => {
    const res = await client.GET('/bili/live/chat/event', {
      params: {
        query: {
          roomId: this.roomId,
          userId: this.userId,
        },
      },
      parseAs: 'stream'
    })
    
    for await (const event of parseServerSentEvents(res.response)) {
      console.log(`事件 ${event.type}`)

      switch (event.type) {
        case 'hot':
          const hot = Number(event.data)
          this._eventSource.dispatchEvent(
            new CustomEvent('hot', { detail: hot }),
          )
          break
        case 'notification':
          const packet = JSON.parse(event.data)
          this.dispatch(packet)
          break
      }
    }
  }

  private static nextId = (): string => crypto.randomUUID()

  private push(message: types.AnyDisplayMessage) {
    this._eventSource.dispatchEvent(
      new CustomEvent('message', { detail: message }),
    )
  }

  private identity(packet: any): types.AuthorType {
    if (packet.info[2][2] == '1') return types.AuthorType.ADMIN
    if (packet.info[2][3] == '1') return types.AuthorType.MEMBER
    // really?
    if (packet.info[2][4] == '1') return types.AuthorType.OWNER
    return types.AuthorType.NORMAL
  }

  private splitContent(packet: any): types.AnyContentPart[] {
    return [
      {
        type: types.ContentPartType.TEXT,
        text: packet.info[1],
      }
    ]
  }

  // damn packet !!!!!!!!!!!!!!!!!!
  private fetchAvatar(uid: string): string {
    return 'placeholder'
  }

  private fetchGiftIcon(giftId: number): string {
    return 'placeholder'
  }

  private dispatch(packet: any) {
    switch (packet.cmd) {
      case 'DANMU_MSG':
        console.log(`收到弹幕: ${packet.info[2][1]}: ${packet.info[1]}`)

        this.push({
          type: types.MessageType.TEXT,
          id: EventClient.nextId(),
          avatarUrl: packet.info[0][15].user.base.face,
          time: new Date(),
          authorName: packet.info[2][1],
          uid: `${packet.info[2][0]}`,
          privilegeType: packet.info[7],
          content: packet.info[1],
          authorType: this.identity(packet),
          contentParts: this.splitContent(packet),
          // TODO: medal info
          medalLevel: 0,
          medalName: '',
        })
        break
      case 'SEND_GIFT':
        this.push({
          type: types.MessageType.GIFT,
          id: EventClient.nextId(),
          time: new Date(),
          avatarUrl: packet.data.face,
          giftName: packet.data.giftName,
          authorName: packet.data.uname,
          uid: packet.data.uid,
          num: packet.data.num,
          giftId: packet.data.giftId,
          price: packet.data.price,
          totalFreeCoin: 0, // or `total_coin`, but no need anymore.
          privilegeType: packet.data.guard_level,
          medalLevel: packet.data.medal_info.medal_level,
          medalName: packet.data.medal_info.medal_name,
          giftIconUrl: this.fetchGiftIcon(packet.data.giftId),
        })
        break
      case 'GUARD_BUY':
        this.push({
          type: types.MessageType.MEMBER,
          id: EventClient.nextId(),
          time: new Date(),
          uid: packet.data.uid,
          authorName: packet.data.username,
          num: packet.data.num,
          privilegeType: packet.data.guard_level,
          price: packet.data.price / 1000,
          avatarUrl: this.fetchAvatar(`${packet.data.uid}`),
          unit: '月', // ...
          // TODO: medal info
          medalLevel: 0,
          medalName: '',
        })
        break
      case 'SUPER_CHAT_MESSAGE':
      case 'SUPER_CHAT_MESSAGE_JP':
        this.push({
          type: types.MessageType.SUPER_CHAT,
          id: EventClient.nextId(),
          time: new Date(),
          content: packet.data.message,
          authorName: packet.data.user_info.uname,
          uid: `${packet.data.uid}`,
          price: packet.data.price,
          avatarUrl: packet.data.user_info.face,
          privilegeType: packet.data.user_info.guard_level,
          medalLevel: packet.data.medal_info.medal_level,
          medalName: packet.data.medal_info.medal_name,
        })
        break
      case 'INTERACT_WORD':
        // TODO: diff from obs including. web browser only.
        break
      case 'WATCHED_CHANGE':
        this._eventSource.dispatchEvent(
          new CustomEvent('viewed', { detail: packet.data.num }),
        )

        break
      // special cases to skip.
      case 'LIVE':
      case 'PREPARING': // LiveEnd by user
      case 'WARNING': // maybe SC implement? (x)
      case 'CUT_OFF': // LiveEnd by admin
      case 'WELCOME':
      case 'WELCOME_GUARD':
      case 'GIFT_TOP':
      default:
        break
    }
  }
}
