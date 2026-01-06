// import * as i18n from '@/i18n'
import { GuardLevel } from './ChatMessageType'
import type {
  AnyDisplayMessage,
  GiftMessage,
  SuperChatMessage,
  TextMessage,
} from './ChatMessageType'

/**
 * 获取等级对应的文字
 * @param guardLevel
 * @deprecated 改用 `GuardLevel.getText`
 */
export const getShowGuardLevelText = (guardLevel: GuardLevel) =>
  GuardLevel.getText(guardLevel)

// /** @deprecated 改为从 ChatMessageType 导入 */
export enum MessageType {
  TEXT = 0,
  GIFT = 1,
  MEMBER = 2,
  SUPER_CHAT = 3,
  DEL = 4,
  UPDATE = 5,
}

// 美元 -> 人民币 汇率
const EXCHANGE_RATE = 7
const PRICE_CONFIGS = [
  // 0 淡蓝
  {
    price: 0,
    colors: {
      contentBg: 'rgba(153, 236, 255, 1)',
      headerBg: 'rgba(153, 236, 255, 1)',
      header: 'rgba(0,0,0,1)',
      authorName: 'rgba(0,0,0,0.701961)',
      time: 'rgba(0,0,0,0.501961)',
      content: 'rgba(0,0,0,1)',
    },
    pinTime: 0,
    priceLevel: 0,
  },
  // ¥0.01 蓝
  {
    price: 0.01,
    colors: {
      contentBg: 'rgba(30,136,229,1)',
      headerBg: 'rgba(21,101,192,1)',
      header: 'rgba(255,255,255,1)',
      authorName: 'rgba(255,255,255,0.701961)',
      time: 'rgba(255,255,255,0.501961)',
      content: 'rgba(255,255,255,1)',
    },
    pinTime: 0,
    priceLevel: 1,
  },
  // $2 浅蓝
  {
    price: 2 * EXCHANGE_RATE,
    colors: {
      contentBg: 'rgba(0,229,255,1)',
      headerBg: 'rgba(0,184,212,1)',
      header: 'rgba(0,0,0,1)',
      authorName: 'rgba(0,0,0,0.701961)',
      time: 'rgba(0,0,0,0.501961)',
      content: 'rgba(0,0,0,1)',
    },
    pinTime: 0,
    priceLevel: 2,
  },
  // $5 绿
  {
    price: 5 * EXCHANGE_RATE,
    colors: {
      contentBg: 'rgba(29,233,182,1)',
      headerBg: 'rgba(0,191,165,1)',
      header: 'rgba(0,0,0,1)',
      authorName: 'rgba(0,0,0,0.541176)',
      time: 'rgba(0,0,0,0.501961)',
      content: 'rgba(0,0,0,1)',
    },
    pinTime: 2,
    priceLevel: 3,
  },
  // $10 黄
  {
    price: 10 * EXCHANGE_RATE,
    colors: {
      contentBg: 'rgba(255,202,40,1)',
      headerBg: 'rgba(255,179,0,1)',
      header: 'rgba(0,0,0,0.87451)',
      authorName: 'rgba(0,0,0,0.541176)',
      time: 'rgba(0,0,0,0.501961)',
      content: 'rgba(0,0,0,0.87451)',
    },
    pinTime: 5,
    priceLevel: 4,
  },
  // $20 橙
  {
    price: 20 * EXCHANGE_RATE,
    colors: {
      contentBg: 'rgba(245,124,0,1)',
      headerBg: 'rgba(230,81,0,1)',
      header: 'rgba(255,255,255,0.87451)',
      authorName: 'rgba(255,255,255,0.701961)',
      time: 'rgba(255,255,255,0.501961)',
      content: 'rgba(255,255,255,0.87451)',
    },
    pinTime: 10,
    priceLevel: 5,
  },
  // $50 品红
  {
    price: 50 * EXCHANGE_RATE,
    colors: {
      contentBg: 'rgba(233,30,99,1)',
      headerBg: 'rgba(194,24,91,1)',
      header: 'rgba(255,255,255,1)',
      authorName: 'rgba(255,255,255,0.701961)',
      time: 'rgba(255,255,255,0.501961)',
      content: 'rgba(255,255,255,1)',
    },
    pinTime: 30,
    priceLevel: 6,
  },
  // $100 红
  {
    price: 100 * EXCHANGE_RATE,
    colors: {
      contentBg: 'rgba(230,33,23,1)',
      headerBg: 'rgba(208,0,0,1)',
      header: 'rgba(255,255,255,1)',
      authorName: 'rgba(255,255,255,0.701961)',
      time: 'rgba(255,255,255,0.501961)',
      content: 'rgba(255,255,255,1)',
    },
    pinTime: 60,
    priceLevel: 7,
  },
]

export const getPriceConfig = (price: number = 0) => {
  let i = 0
  // 根据先验知识，从小找到大通常更快结束
  for (; i < PRICE_CONFIGS.length - 1; i++) {
    let nextConfig = PRICE_CONFIGS[i + 1]
    if (price < nextConfig.price) {
      return PRICE_CONFIGS[i]
    }
  }
  return PRICE_CONFIGS[i]
}

export const getShowContent = (
  message: AnyDisplayMessage,
): string | undefined => {
  if ('translation' in message)
    return `${message.content}（${message.translation}）`

  return (message as any)?.content
}

export const getShowContentParts = (message: TextMessage) => {
  let contentParts = [...message.contentParts]
  if (message.translation) {
    contentParts.push({
      type: ContentPartType.TEXT,
      text: `（${message.translation}）`,
    })
  }
  return contentParts
}

export const getGiftShowContent = (
  message: GiftMessage,
  showGiftName?: boolean,
) => {
  if (!showGiftName) return ''

  return `赠送 ${getGiftShowNameAndNum(message)}`
}

export const getGiftShowNameAndNum = (message: GiftMessage) =>
  `${message.giftName}x${message.num}`

export const getShowAuthorName = (message: AnyDisplayMessage) => {
  if (
    'authorNamePronunciation' in message &&
    message.authorNamePronunciation !== message.authorName
  )
    return `${message.authorName}(${message.authorNamePronunciation})`

  return message.authorName
}

export const DEFAULT_AVATAR_URL = '//static.hdslb.com/images/member/noface.gif'
