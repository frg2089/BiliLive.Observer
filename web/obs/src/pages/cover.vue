<template>
  <div>
    <div flex gap-2>
      <CoverCropper :image="url" :in-bounds="inBounds" flex-1 />

      <div>
        <div>16:9</div>
        <CoverCropperViewer :width="480" :height="270" />

        <div>4:3</div>
        <CoverCropperViewer :width="480" :height="360" />
      </div>
    </div>
    <div mx-auto>
      <NSwitch v-model:value="inBounds" />
      <NButton type="primary">提交</NButton>
      <NButton type="error">取消</NButton>
    </div>
  </div>
</template>

<script lang="ts" setup>
const route = useRoute()
const url = ref('')
const inBounds = ref(true)

onMounted(() => {
  url.value = route.query.url as string
})
</script>
