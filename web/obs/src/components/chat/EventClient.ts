import { SSE } from 'sse.js' // TODO: parse-sse

import { pathChecker } from '../../api'
import * as types from './ChatMessageType'

export class EventClient {
  private sse: SSE

  /** 人气值 */
  hot = ref(0)

  /** 历史观看人数 */
  viewed = ref(0)

  /** 外显消息队列 */
  messages = reactive<types.AnyDisplayMessage[]>([])

  constructor(roomId: string, userId: number) {
    this.sse = new SSE(
      pathChecker(`/bili/live/chat/event?roomId=${roomId}&userId=${userId}`),
    )
    this.sse.addEventListener('hot', (e: CustomEvent & { data: string }) => {
      this.hot.value = Number(e.data)
    })
    this.sse.addEventListener(
      'notification',
      (e: CustomEvent & { data: string }) => {
        const packet = JSON.parse(e.data)
        this.dispatch(packet)
      },
    )
  }

  private static nextId = (): string => crypto.randomUUID()

  private push(message: types.AnyDisplayMessage) {
    // TODO
  }

  private identity(packet: any): types.AuthorType {
    if (packet.info[2][2] == '1') return types.AuthorType.ADMIN
    if (packet.info[2][3] == '1') return types.AuthorType.MEMBER
    // really?
    if (packet.info[2][4] == '1') return types.AuthorType.OWNER
    return types.AuthorType.NORMAL
  }

  private splitContent(packet: any): types.AnyContentPart[] {
    return []
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
        this.viewed.value = packet.data.num
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
