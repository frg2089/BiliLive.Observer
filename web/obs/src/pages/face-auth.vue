<template>
  <div v-if="'qr' in route.query && typeof route.query.qr === 'string'">
    <div relative mb-4 w-full>
      <h2 absolute top-0 my-2 w-full text-center text-lg>OBS 设置</h2>
      <RouterLink to="/">
        <NButton type="primary" text> 返回首页 </NButton>
      </RouterLink>
    </div>
    <div
      ref="box"
      :class="{ 'login-qr-code': zoom <= 1 }"
      flex
      justify-center
      items-center
      mx-auto>
      <NQrCode
        :value="route.query.qr"
        :size
        :padding="size / 32"
        color="#ff6699"
        background-color="#00000000"
        type="svg"
        error-correction-level="H"
        justify-center
        items-center />
    </div>
    <div v-if="size * zoom < 100">
      太小看不清？按住
      <kbd>Ctrl</kbd> + <kbd>鼠标滚轮</kbd>
      即可缩放
    </div>
  </div>
</template>

<script lang="ts" setup>
const route = useRoute()

const box = ref<HTMLElement>()
const size = ref(150)
const zoom = ref(1)

onMounted(async () => {
  if (!box.value) return
  const observer = new ResizeObserver(() => {
    if (!box.value) return
    zoom.value = window.devicePixelRatio
    size.value = Math.min(155, (box.value.clientWidth / 480) * 155 * zoom.value)
  })
  observer.observe(box.value)
})
</script>
