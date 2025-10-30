declare namespace Bili.Live.Danmaku {
  export type DanmakuInfo = [
    data: [
      unknown,
      mode: number,
      fontSize: number,
      color: number,
      timestamp: number,
      unknown,
      unknown,
      unknown,
      unknown,
      unknown,
      unknown,
      unknown,
      unknown,
      unknown,
      unknown,
      data: {
        extra: string
        mode: number
        show_player_type: number
        user: {
          base: {
            face: string
            is_mystery: boolean
            name: string
            name_color: number
            name_color_str: number
            official_info: {
              desc: number
              role: number
              title: number
              type: number
            }
            origin_info: {
              face: string
              name: string
            }
            risk_ctrl_info: null
          }
          guard: null
          guard_leader: { is_guard_leader: boolean }
          medal: null
          title: {
            old_title_css_id: number
            title_css_id: number
          }
          uhead_frame: null
          uid: number
          wealth: null
        }
      },
      activity: {
        activity_identity: string
        activity_source: number
        not_show: number
      },
    ],
    text: string,
    senderInfo: [
      userId: number,
      userName: string,
      number,
      number,
      number,
      number,
      number,
      string,
    ],
    senderBadge: any[],
    senderUL: [number, number, number, string, number],
    title: [string, string],
    unknown,
    unknown,
    null,
    timestamp: {
      ts: number
      ct: string
    },
    unknown,
    unknown,
    null,
    null,
  ]

  export interface BaseEvent {
    type: string
  }

  // 直播开始
  export interface LiveStart extends BaseEvent {
    type: 'LiveStart'
    roomID: number | string
  }

  // 直播结束
  export interface LiveEnd extends BaseEvent {
    type: 'LiveEnd'
    roomID?: number | string
    commentText?: string // CUT_OFF 特有
  }

  // 弹幕评论
  export interface Comment extends BaseEvent {
    type: 'Comment'
    commentText: string
    userID: number
    userAvatar: string
    userName: string
    isAdmin: boolean
    isVIP: boolean
    userGuardLevel: number
  }

  // 礼物发送
  export interface GiftSend extends BaseEvent {
    type: 'GiftSend'
    giftName: string
    userName: string
    userID: number
    giftCount: number
  }

  // 礼物排行榜
  export interface GiftTop extends BaseEvent {
    type: 'GiftTop'
    giftRanking: Array<{
      uid: number
      userName: string
      coin: number
    }>
  }

  // 普通欢迎用户
  export interface Welcome extends BaseEvent {
    type: 'Welcome'
    userName: string
    userID: number
    isVIP: true
    isAdmin: boolean
  }

  // 守护欢迎用户
  export interface WelcomeGuard extends BaseEvent {
    type: 'WelcomeGuard'
    userName: string
    userID: number
    userGuardLevel: number
  }

  // 用户购买守护
  export interface GuardBuy extends BaseEvent {
    type: 'GuardBuy'
    userID: number
    userName: string
    userGuardLevel: number
    giftName: string
    giftCount: number
  }

  // 醒目留言 Super Chat
  export interface SuperChat extends BaseEvent {
    type: 'SuperChat'
    commentText: string
    userID: number
    userName: string
    price: number
    scKeepTime: number
  }

  // 互动事件（如进入直播）
  export interface Interact extends BaseEvent {
    type: 'Interact'
    userName: string
    userID: number
    interactType: number
  }

  // 警告信息
  export interface Warning extends BaseEvent {
    type: 'Warning'
    commentText: string
  }

  // 观看人数变化
  export interface WatchedChange extends BaseEvent {
    type: 'WatchedChange'
    watchedCount: number
  }

  export type Event =
    | LiveStart
    | LiveEnd
    | Comment
    | GiftSend
    | GiftTop
    | Welcome
    | WelcomeGuard
    | GuardBuy
    | SuperChat
    | Interact
    | Warning
    | WatchedChange
}
