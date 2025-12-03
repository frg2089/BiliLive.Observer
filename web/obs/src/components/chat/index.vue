<template>
  <yt-live-chat-renderer
    class="style-scope yt-live-chat-app"
    style="--scrollbar-width: 11px"
    hide-timestamps
    @mousemove="refreshCantScrollStartTime">
    <Ticker
      class="style-scope yt-live-chat-renderer"
      v-model:messages="paidMessages"
      :show-gift-name="showGiftName" />
    <yt-live-chat-item-list-renderer
      class="style-scope yt-live-chat-renderer"
      allow-scroll>
      <div
        ref="scroller"
        id="item-scroller"
        class="style-scope yt-live-chat-item-list-renderer animated"
        @scroll="onScroll">
        <div
          ref="itemOffset"
          id="item-offset"
          class="style-scope yt-live-chat-item-list-renderer">
          <div
            ref="items"
            id="items"
            class="style-scope yt-live-chat-item-list-renderer"
            style="overflow: hidden"
            :style="{
              transform: `translateY(${Math.floor(scrollPixelsRemaining)}px)`,
            }">
            <ChatItemBox
              v-for="message in messages"
              :key="message.id"
              :show-gift-name="props.showGiftName"
              :message="message" />
          </div>
        </div>
      </div>
    </yt-live-chat-item-list-renderer>
  </yt-live-chat-renderer>
</template>

<script lang="ts" setup>
import * as types from './ChatMessageType'
import * as constants from './constants'
import { DanmakuQueue } from './DanmakuQueue'

const scroller = useTemplateRef('scroller')
const itemOffset = useTemplateRef('itemOffset')
const items = useTemplateRef('items')

// 发送消息时间间隔范围
const MESSAGE_MIN_INTERVAL = 80
const MESSAGE_MAX_INTERVAL = 1000

// 每次发送消息后增加的动画时间，要比MESSAGE_MIN_INTERVAL稍微大一点，太小了动画不连续，太大了发送消息时会中断动画
// 84 = ceil((1000 / 60) * 5)
const CHAT_SMOOTH_ANIMATION_TIME_MS = 84
// 滚动条距离底部小于多少像素则认为在底部
const SCROLLED_TO_BOTTOM_EPSILON = 15

const props = withDefaults(
  defineProps<{
    maxNumber: number
    showGiftName?: boolean
  }>(),
  {
    maxNumber: 60,
  },
)

// TODO: logic implements
const msgQueue = new DanmakuQueue()
const scrollPixelsRemaining = ref<number>(0)
const cantScrollStartTime = ref<Date>()

const messages = msgQueue.messages
const paidMessages = computed(
  () =>
    messages.filter(i => i.type !== types.MessageType.TEXT) as Exclude<
      types.AnyDisplayMessage,
      types.TextMessage
    >,
)

const onScroll = () => {
  // TODO: ...
}
const refreshCantScrollStartTime = () => {
  if (!cantScrollStartTime.value) return

  cantScrollStartTime.value = new Date()
}
</script>

<!-- <style src="@/assets/css/youtube/yt-html.css"></style>
<style src="@/assets/css/youtube/yt-live-chat-renderer.css"></style>
<style src="@/assets/css/youtube/yt-live-chat-item-list-renderer.css"></style> -->
