<template>
  <div flex h-screen>
    <NInput
      flex-1
      v-model:value="text"
      placeholder="发送弹幕"
      :disabled="sending"
      :loading="sending"
      @keydown="keydown" />
    <NButton
      h-screen
      type="primary"
      :disabled="sending"
      :loading="sending"
      @click="send">
      发送
    </NButton>
  </div>
</template>

<script lang="ts" setup>
import { client } from '../../../api'

interface Props {
  roomId: number
}

definePage({
  props: route => {
    const result: Props = { roomId: 0 }

    if ('roomId' in route.params) result.roomId = Number(route.params.roomId)

    return result
  },
})

const props = defineProps<Props>()

const text = ref('')
const sending = ref(false)
const send = async () => {
  if (sending.value) return
  sending.value = true
  try {
    await client.POST('/bili/live/chat/send', {
      body: {
        roomId: props.roomId,
        message: text.value,
      },
    })
  } finally {
    sending.value = false
    text.value = ''
  }
}

const keydown = (e: KeyboardEvent) => {
  if (e.key === 'Enter') send()
}
</script>
