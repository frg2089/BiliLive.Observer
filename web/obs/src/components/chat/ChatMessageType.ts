/**
 * @description 消息类型
 */
export enum MessageType {
  /**
   * @description 评论 @see TextMessage
   */
  TEXT = 0,
  /**
   * @description 礼物 @see GiftMessage
   */
  GIFT = 1,
  /**
   * @description 上舰 @see MemberMessage
   */
  MEMBER = 2,
  /**
   * @description 醒目留言 @see SuperChatMessage
   */
  SUPER_CHAT = 3,
}

/**
 * @description 作者类型
 */
export enum AuthorType {
  NORMAL = 0,
  /**
   * @description 舰队
   */
  MEMBER = 1,
  /**
   * @description 房管
   */
  ADMIN = 2,
  /**
   * @description 主播
   */
  OWNER = 3,
}

export const AuthorTypeText = {
  [AuthorType.NORMAL]: '',
  [AuthorType.MEMBER]: 'member',
  [AuthorType.ADMIN]: 'moderator',
  [AuthorType.OWNER]: 'owner',
}

/**
 * @description 舰队等级。因为历史原因，消息里的字段名叫`privilegeType`
 */
export enum GuardLevel {
  NONE = 0,
  /**
   * @description 总督
   */
  LV3 = 1,
  /**
   * @description 提督
   */
  LV2 = 2,
  /**
   * @description 舰长
   */
  LV1 = 3,
}
export namespace GuardLevel {
  export const GuardLevelText = {
    [GuardLevel.NONE]: '',
    [GuardLevel.LV3]: '总督',
    [GuardLevel.LV2]: '提督',
    [GuardLevel.LV1]: '舰长',
  }
  export const getText = (level: GuardLevel) => GuardLevelText[level] ?? ''
}

/**
 * @description 一段内容的类型
 */
export enum ContentPartType {
  /**
   * @description 文本
   */
  TEXT = 0,
  /**
   * @description 图片
   */
  IMAGE = 1,
}
export namespace ContentPartType {
  /**
   * @deprecated 直接用 part.type === ContentPartType.TEXT 比较
   */
  export const isText = (part: AnyContentPart): part is TextContentPart =>
    part.type === ContentPartType.TEXT
  /**
   * @deprecated 直接用 part.type === ContentPartType.IMAGE 比较
   */
  export const isImage = (part: AnyContentPart): part is ImageContentPart =>
    part.type === ContentPartType.IMAGE
}

/**
 * @description 用于显示的消息
 */
export type AnyDisplayMessage =
  | TextMessage
  | GiftMessage
  | MemberMessage
  | SuperChatMessage

export type MessageId<T extends AnyDisplayMessage> = T extends { id: string }
  ? T['id']
  : never

export interface MessageBase {
  /**
   * @description 消息ID
   */
  id: string
  /**
   * @description 消息类型
   */
  type: MessageType
  /**
   * @description 用户头像URL
   */
  avatarUrl: string
  /**
   * @description 时间
   */
  time: Date
  /**
   * @description 用户名
   */
  authorName: string

  /**
   * @description 用户Open ID或ID，使用房间ID连接时不保证是唯一的
   */
  uid: string
  /**
   * @description 舰队等级
   */
  privilegeType: GuardLevel
  /**
   * @description 勋章等级，如果没戴当前房间勋章则为0
   */
  medalLevel: number
  /**
   * @description 勋章名，如果没戴当前房间勋章则为空字符串
   */
  medalName: string

  /**
   * @deprecated
   */
  $add?: Partial<AnyDisplayMessage>
  /**
   * @deprecated
   */
  title?: string
  /**
   * @deprecated
   */
  repeated?: number
  /**
   * @deprecated
   */
  addTime?: Date
}

/**
 * @description 评论消息。因为历史原因叫 TextMessage ，实际上会包含表情图片
 */
export interface TextMessage extends MessageBase {
  /**
   * @description 消息类型
   */
  type: MessageType.TEXT
  /**
   * @description 用户类型
   */
  authorType: AuthorType
  /**
   * @description 纯文本表示的内容
   */
  content: string
  /**
   * @description 解析后的内容，包括文本、图片
   */
  contentParts: AnyContentPart[]
  /**
   * @description 内容的翻译，刚添加时一般是空的，之后通过更新消息赋值
   */
  translation?: string
}

/**
 * @description 一段内容
 */
export type AnyContentPart = TextContentPart | ImageContentPart

/**
 * @description 一段文本内容
 */
export interface TextContentPart {
  /**
   * @description 内容类型
   */
  type: ContentPartType.TEXT
  /**
   * @description 内容
   */
  text: string
}

/**
 * @description 一段图片内容
 */
export interface ImageContentPart {
  /**
   * @description 内容类型
   */
  type: ContentPartType.IMAGE
  /**
   * @description 内容
   */
  text: string
  /**
   * @description 图片URL
   */
  url: string
  /**
   * @description 宽度，加载失败则为0
   */
  width: number
  /**
   * @description 高度，加载失败则为0
   */
  height: number
}

/**
 * @description 礼物消息
 */
export interface GiftMessage extends MessageBase {
  /**
   * @description 消息类型
   */
  type: MessageType.GIFT
  /**
   * @description 用户名读音
   */
  authorNamePronunciation?: string
  /**
   * @description 总价（元），免费礼物则为0
   */
  price: number
  /**
   * @description 礼物名
   */
  giftName: string
  /**
   * @description 数量
   */
  num: number
  /**
   * @description 免费礼物总价（银瓜子数），付费礼物则为0
   */
  totalFreeCoin: number
  /**
   * @description 礼物ID
   */
  giftId: number
  /**
   * @description 礼物图标URL
   */
  giftIconUrl: string
}

/**
 * @description 上舰消息
 */
export interface MemberMessage extends MessageBase {
  /**
   * @description 消息类型
   */
  type: MessageType.MEMBER
  /**
   * @description 用户名读音
   */
  authorNamePronunciation?: string
  /**
   * @description 数量
   */
  num: number
  /**
   * @description 单位（"月"）
   */
  unit: string
  /**
   * @description 总价（元）
   */
  price: number
}

/**
 * @description 醒目留言消息
 */
export interface SuperChatMessage extends MessageBase {
  /**
   * @description 消息类型
   */
  type: MessageType.SUPER_CHAT
  /**
   * @description 用户名读音
   */
  authorNamePronunciation?: string
  /**
   * @description 价格（元）
   */
  price: number
  /**
   * @description 内容
   */
  content: string
  /**
   * @description 内容的翻译，刚添加时一般是空的，之后通过更新消息赋值
   */
  translation?: string
}
