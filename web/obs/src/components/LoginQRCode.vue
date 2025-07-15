<template>
  <div class="login-qr-code" flex justify-center items-center>
    <QrcodeVue
      :value="data?.url"
      :foreground="mask ? '#333' : '#ff6699'"
      background="#00000000"
      render-as="svg"
      :size="150"
      @click="init" />
  </div>
</template>

<script lang="ts" setup>
import QrcodeVue from 'qrcode.vue'

import type { components, paths } from '../../obj/apis'
import { client } from '../api'

const emit = defineEmits<{
  (e: 'success'): void
}>()

const data = ref<{
  url: string
  qrcode_key: string
}>()
const status = ref<number>()
const mask = ref(false)
let handle: number

const init = async () => {
  if (handle) clearInterval(handle)
  mask.value = true
  const res = await client.GET('/bili/login')
  data.value = res.data
  handle = setInterval(poll, 1000)
}

const poll = async () => {
  if (!data.value) return

  const res = await client.GET('/bili/login/poll', {
    params: {
      query: {
        qrcodeKey: data.value.qrcode_key,
      },
    },
  })

  if (res.error) {
    if (handle) clearInterval(handle)
    console.error(res)
  }

  status.value = res.data

  switch (res.data) {
    case 0: // 未扫码
      mask.value = false
      break
    case 1: // 已扫码
      mask.value = true
      break
    case 2: // 已登录
      if (handle) clearInterval(handle)
      emit('success')
      break
  }
}

onMounted(init)

onUnmounted(() => {
  clearInterval(handle)
})

onDeactivated(() => {
  clearInterval(handle)
})
</script>

<style>
.login-qr-code {
  width: 480px;
  height: 155px;
  background-image: url('../assets/2233login.webp');
  background-position: center;
  background-size: cover;
  background-repeat: no-repeat;
}
</style>
