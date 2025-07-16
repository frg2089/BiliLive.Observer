<template>
  <div>
    <h2 text-center text-lg>登录</h2>
    <NSpin :show="loading" mx-auto>
      <div
        ref="box"
        :class="{ 'login-qr-code': zoom <= 1 }"
        flex
        justify-center
        items-center
        mx-auto>
        <NQrCode
          :value="data?.url"
          :size
          :icon-src="imgurl"
          :icon-size="size / 4"
          :padding="size / 32"
          color="#ff6699"
          background-color="#00000000"
          type="svg"
          error-correction-level="H"
          justify-center
          items-center
          @click="init"
          @mouseover="hover = true"
          @mouseout="hover = false" />
      </div>
      <div v-if="size * zoom < 100">
        太小看不清？按住
        <kbd>Ctrl</kbd> + <kbd>鼠标滚轮</kbd>
        即可缩放
      </div>
    </NSpin>
  </div>
</template>

<script lang="ts" setup>
import { client } from '../api'
import logo from '../assets/logo.svg'
import refresh from '../assets/refresh.svg'
import success from '../assets/success.svg'

const router = useRouter()
const loading = ref(false)
const hover = ref(false)
const scanned = ref(false)
const box = ref<HTMLElement>()
const size = ref(150)
const zoom = ref(1)

const imgurl = computed(() => {
  if (hover.value) return refresh
  if (scanned.value) return success
  return logo
})

const data = ref<{
  url: string
  qrcode_key: string
}>()
let handle: number

const init = async () => {
  loading.value = true
  try {
    clearInterval(handle)
    scanned.value = false
    const res = await client.GET('/bili/login')
    data.value = res.data
    handle = setInterval(poll, 1000)
  } finally {
    loading.value = false
  }
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
    clearInterval(handle)
    console.error(res)
  }

  switch (res.data) {
    case 0: // 未扫码
      scanned.value = false
      break
    case 1: // 已扫码
      scanned.value = true
      break
    case 2: // 已登录
      if (handle) clearInterval(handle)
      router.push('/')
      break
  }
}

onMounted(async () => {
  init()

  if (!box.value) return
  const observer = new ResizeObserver(() => {
    if (!box.value) return
    zoom.value = window.devicePixelRatio
    size.value = Math.min(155, (box.value.clientWidth / 480) * 155 * zoom.value)
  })
  observer.observe(box.value)
})

onUnmounted(() => clearInterval(handle))
</script>

<style>
.login-qr-code {
  max-width: 480px;
  background-image: url('../assets/2233login.webp');
  background-position: center;
  background-size: contain;
  background-repeat: no-repeat;
}
</style>
