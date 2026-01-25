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
import * as types from '../../../components/chat/ChatMessageType'
import { EventClient } from '../../../components/chat/EventClient'

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
    props.roomId,
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


</script>

<!-- TODO: 可以考虑薅一下流通的油管风格 css -->
<!-- <style src="@/assets/css/youtube/yt-html.css"></style>
<style src="@/assets/css/youtube/yt-live-chat-renderer.css"></style>
<style src="@/assets/css/youtube/yt-live-chat-item-list-renderer.css"></style> -->
