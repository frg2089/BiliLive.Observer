<template>
  <yt-live-chat-renderer
    class="style-scope yt-live-chat-app"
    style="--scrollbar-width: 11px"
    absolute
    bottom-0
    max-h-screen
    w-full
    overflow-hidden>
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
        class="style-scope yt-live-chat-item-list-renderer animated">
        <div
          ref="itemOffset"
          id="item-offset"
          class="style-scope yt-live-chat-item-list-renderer">
          <div
            ref="items"
            id="items"
            class="style-scope yt-live-chat-item-list-renderer"
            max-h-screen
            overflow-y-scroll>
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
import * as types from '../../components/chat/ChatMessageType'
import { EventClient } from '../../components/chat/EventClient'

interface Props {
  maxNumber?: number
  showGiftName?: boolean
  useCss?: string
  roomId: number
}

definePage({
  props: route => {
    const result: Props = { roomId: 0 }

    if ('roomId' in route.params) result.roomId = Number(route.params.roomId)
    if (route.query.showGiftName)
      result.showGiftName = Boolean(route.query.showGiftName)
    if (route.query.maxNumber) result.maxNumber = Number(route.query.maxNumber)
    if (route.query.useCss) result.useCss = route.query.useCss.toString()

    return result
  },
})

const props = withDefaults(defineProps<Props>(), {
  maxNumber: 60,
})

const user = useUser()
const hot = ref(0)
const viewed = ref(0)
const messages = reactive<types.AnyDisplayMessage[]>([])
const paidMessages = reactive<
  Array<Exclude<types.AnyDisplayMessage, types.TextMessage>>
>([])

const items = useTemplateRef('items')

const init = async () => {
  if (props.useCss) {
    const link = document.createElement('link')
    link.rel = 'stylesheet'
    link.href = props.useCss
    document.head.appendChild(link)
  }

  await user.updateUserInfo()
  const eventClient = new EventClient(
    props.roomId ?? user.roomId ?? 0,
    user.userId ?? 0,
  )
  eventClient.addEventListener('hot', e => (hot.value = e.detail))
  eventClient.addEventListener('viewed', e => (viewed.value = e.detail))
  eventClient.addEventListener('message', async e => {
    const message = e.detail
    messages.push(message)
    if (message.type !== types.MessageType.TEXT && message.price > 0) {
      paidMessages.push(message)
      setTimeout(
        () => paidMessages.shift(),
        (5 + 0.05 * message.price * 1000) * 1000,
      )
    }
    if (messages.length > props.maxNumber) messages.shift()

    await nextTick()

    items.value?.lastElementChild?.scrollIntoView({
      behavior: 'smooth',
      block: 'center',
    })
  })
  eventClient.start()
}
onMounted(init)

/* original blivechat scrolling control logic

const scroller = useTemplateRef('scroller')
const itemOffset = useTemplateRef('itemOffset')

// 发送消息时间间隔范围
const MESSAGE_MIN_INTERVAL = 80
const MESSAGE_MAX_INTERVAL = 1000

// 每次发送消息后增加的动画时间，要比MESSAGE_MIN_INTERVAL稍微大一点，太小了动画不连续，太大了发送消息时会中断动画
// 84 = ceil((1000 / 60) * 5)
const CHAT_SMOOTH_ANIMATION_TIME_MS = 84
// 滚动条距离底部小于多少像素则认为在底部
const SCROLLED_TO_BOTTOM_EPSILON = 15

const atBottom = ref<boolean>(true)
const scrollPixelsRemaining = ref<number>(0)
const cantScrollStartTime = ref<Date>()

const onScroll = () => {
  refreshCantScrollStartTime()
  atBottom = scroller.value.scrollHeight - scroller.value.scrollTop - scroller.value.clientHeight < SCROLLED_TO_BOTTOM_EPSILON
  // flushMessagesBuffer()
}
const flushMessagesBuffer = async () => {
  if (this.messagesBuffer.length <= 0) {
    return
  }
  if (!this.canScrollToBottomOrTimedOut()) {
    if (this.messagesBuffer.length > this.maxNumber) {
      // 未显示消息数 > 最大可显示数，丢弃
      this.messagesBuffer.splice(0, this.messagesBuffer.length - this.maxNumber)
    }
    return
  }

  let removeNum = Math.max(this.messages.length + this.messagesBuffer.length - this.maxNumber, 0)
  if (removeNum > 0) {
    this.messages.splice(0, removeNum)
    // 防止同时添加和删除项目时所有的项目重新渲染 https://github.com/vuejs/vue/issues/6857
    await this.$nextTick()
  }

  this.preinsertHeight = this.$refs.items.clientHeight
  for (let message of this.messagesBuffer) {
    this.messages.push(message)
  }
  this.messagesBuffer = []
  // 等items高度变化
  await this.$nextTick()
  this.showNewMessages()
}
const refreshCantScrollStartTime = () => {
  if (!cantScrollStartTime.value) return

  cantScrollStartTime.value = new Date()
}

**/
</script>

<!-- <style src="@/assets/css/youtube/yt-html.css"></style>
<style src="@/assets/css/youtube/yt-live-chat-renderer.css"></style>
<style src="@/assets/css/youtube/yt-live-chat-item-list-renderer.css"></style> -->
