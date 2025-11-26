<template>
  <yt-live-chat-ticker-renderer :hidden="showMessages.length === 0">
    <NScrollbar
      ref="scrollbar"
      id="container"
      dir="ltr"
      class="style-scope yt-live-chat-ticker-renderer">
      <TransitionGroup
        tag="div"
        :css="false"
        @enter="onTickerItemEnter"
        @leave="onTickerItemLeave"
        id="items"
        class="style-scope yt-live-chat-ticker-renderer">
        <yt-live-chat-ticker-paid-message-item-renderer
          v-for="message in showMessages"
          :key="message.raw.id"
          tabindex="0"
          class="style-scope yt-live-chat-ticker-renderer"
          style="overflow: hidden"
          @click="onItemClick(message.raw)">
          <div
            id="container"
            dir="ltr"
            class="style-scope yt-live-chat-ticker-paid-message-item-renderer"
            :style="{
              background: message.bgColor,
            }">
            <div
              id="content"
              class="style-scope yt-live-chat-ticker-paid-message-item-renderer"
              :style="{
                color: message.color,
              }">
              <ImgShadow
                id="author-photo"
                height="24"
                width="24"
                class="style-scope yt-live-chat-ticker-paid-message-item-renderer"
                :imgUrl="message.raw.avatarUrl" />
              <span
                id="text"
                dir="ltr"
                class="style-scope yt-live-chat-ticker-paid-message-item-renderer">
                {{ message.text }}
              </span>
            </div>
          </div>
        </yt-live-chat-ticker-paid-message-item-renderer>
      </TransitionGroup>
    </NScrollbar>
    <template v-if="pinnedMessage" :key="pinnedMessage.id">
      <MembershipItem
        v-if="pinnedMessage.type === types.MessageType.MEMBER"
        class="style-scope yt-live-chat-ticker-renderer"
        :avatarUrl="pinnedMessage.avatarUrl"
        :authorName="getShowAuthorName(pinnedMessage)"
        :privilegeType="pinnedMessage.privilegeType"
        :title="pinnedMessage.title"
        :time="pinnedMessage.time" />
      <PaidMessage
        v-else
        class="style-scope yt-live-chat-ticker-renderer"
        :price="pinnedMessage.price"
        :priceText="''"
        :avatarUrl="pinnedMessage.avatarUrl"
        :authorName="getShowAuthorName(pinnedMessage)"
        :time="pinnedMessage.time"
        :content="pinnedMessageShowContent" />
    </template>
  </yt-live-chat-ticker-renderer>
</template>

<script lang="ts" setup>
// import * as chatConfig from '@/api/chatConfig'
// import { formatCurrency } from '@/utils'
import { TransitionGroup } from 'vue'
import { NScrollbar } from 'naive-ui'

import * as types from '../../types/ChatMessageType'
import {
  getGiftShowContent,
  getPriceConfig,
  getShowAuthorName,
  getShowContent,
} from './constants'
import ImgShadow from './ImgShadow.vue'
import MembershipItem from './MembershipItem.vue'
import PaidMessage from './PaidMessage.vue'

const scrollbar = useTemplateRef('scrollbar')

const props = withDefaults(
  defineProps<{
    messages: Exclude<types.AnyDisplayMessage, types.TextMessage>[]
    showGiftName: boolean
  }>(),
  {
    showGiftName: false,
  },
)

const emit = defineEmits<{
  (
    e: 'update:messages',
    value: Exclude<types.AnyDisplayMessage, types.TextMessage>[],
  ): void
}>()

const showMessages = computed(() => {
  let res = []
  for (let message of props.messages) {
    if (!needToShow(message)) {
      continue
    }
    res.push({
      raw: message,
      bgColor: getBgColor(message),
      color: getColor(message),
      text: getText(message),
    })
  }
  return res
})
const pinnedMessageShowContent = computed(() => {
  if (!pinnedMessage.value) {
    return ''
  }
  if (pinnedMessage.value.type === types.MessageType.GIFT) {
    return getGiftShowContent(pinnedMessage.value, props.showGiftName)
  } else {
    return getShowContent(pinnedMessage.value)
  }
})

onBeforeUnmount(() => {
  window.clearInterval(updateTimerId.value)
})

const onTickerItemEnter = async (_el: Element, done: () => void) => {
  const el = _el as HTMLElement

  let width = el.clientWidth
  if (width === 0) {
    // CSS指定了不显示固定栏
    done()
    return
  }
  el.style.width = '0px'
  await nextTick()
  el.style.width = `${width}px`

  await delay(200)
  done()

  // 因为el-scrollbar监听不到事件，这里必须手动调update
  // Ag：NScrollBar 有必要马
  scrollbar.value?.$forceUpdate()
  // this.$refs.scrollbar.update()
}

const onTickerItemLeave = async (_el: Element, done: () => void) => {
  const el = _el as HTMLElement
  el.classList.add('sliding-down')

  await delay(200)

  el.classList.add('collapsing')
  el.style.width = '0px'

  await delay(200)

  el.classList.remove('sliding-down')
  el.classList.remove('collapsing')
  el.style.width = 'auto'
  done()

  // 因为el-scrollbar监听不到事件，这里必须手动调update
  // Ag：同上
  scrollbar.value?.$forceUpdate()
  // this.$refs.scrollbar.update()
}
const needToShow = (
  message: Exclude<types.AnyDisplayMessage, types.TextMessage>,
) =>
  (new Date().getTime() - message.time.getTime()) / (60 * 1000) <
  getPinTime(message)
const getBgColor = (
  message: Exclude<types.AnyDisplayMessage, types.TextMessage>,
) => {
  let color1, color2
  if (message.type === types.MessageType.MEMBER) {
    color1 = 'rgba(15,157,88,1)'
    color2 = 'rgba(11,128,67,1)'
  } else {
    const config = getPriceConfig(message.price)
    color1 = config.colors.contentBg
    color2 = config.colors.headerBg
  }
  let pinTime = getPinTime(message)
  let progress =
    (1 -
      (curTime.value.getTime() - message.time.getTime()) /
        (60 * 1000) /
        pinTime) *
    100
  if (progress < 0) {
    progress = 0
  } else if (progress > 100) {
    progress = 100
  }
  return `linear-gradient(90deg, ${color1}, ${color1} ${progress}%, ${color2} ${progress}%, ${color2})`
}
const getColor = (
  message: Exclude<types.AnyDisplayMessage, types.TextMessage>,
) =>
  message.type === types.MessageType.TEXT
    ? 'rgb(255,255,255)'
    : getPriceConfig(message.price).colors.header
const getText = (
  message: Exclude<types.AnyDisplayMessage, types.TextMessage>,
) =>
  message.type === types.MessageType.TEXT
    ? '会员'
    : `CN¥${formatCurrency(message.price)}`
const getPinTime = (
  message: Exclude<types.AnyDisplayMessage, types.TextMessage>,
) =>
  message.type === types.MessageType.TEXT
    ? 2
    : getPriceConfig(message.price).pinTime
const updateProgress = () => {
  // 更新进度
  curTime.value = new Date()

  // 删除过期的消息
  let filteredMessages = []
  let messagesChanged = false
  for (let message of props.messages) {
    let pinTime = getPinTime(message)
    if (
      (curTime.value.getTime() - message.time.getTime()) / (60 * 1000) >=
      pinTime
    ) {
      messagesChanged = true
      if (pinnedMessage.value === message) {
        pinnedMessage.value = undefined
      }
      continue
    }
    filteredMessages.push(message)
  }
  if (messagesChanged) {
    emit('update:messages', filteredMessages)
  }
}
const onItemClick = (
  message: Exclude<types.AnyDisplayMessage, types.TextMessage>,
) => {
  if (pinnedMessage.value === message) {
    pinnedMessage.value = undefined
  } else {
    pinnedMessage.value = message
  }
}

const curTime = ref(new Date())
const updateTimerId = ref(window.setInterval(updateProgress, 1000))
const pinnedMessage = ref<Exclude<types.AnyDisplayMessage, types.TextMessage>>() // really?
</script>

<!-- <style src="@/assets/css/youtube/yt-live-chat-ticker-renderer.css"></style>
<style src="@/assets/css/youtube/yt-live-chat-ticker-paid-message-item-renderer.css"></style> -->
