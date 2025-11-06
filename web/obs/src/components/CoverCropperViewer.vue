<template>
  <div
    flex
    justify-center
    items-center
    overflow-hidden
    :style="{
      width: `${width}px`,
      height: `${height}px`,
    }">
    <div
      :style="{
        width: `${innerWidth}px`,
        height: `${height}px`,
      }">
      <cropper-viewer selection="#cropperSelection" resize="horizontal" />
    </div>
  </div>
</template>

<script lang="ts" setup>
const props = defineProps<{
  width: number
  height?: number
}>()
const a = 16 / 9
const height = computed(() => props.height ?? props.width * a)
const innerWidth = computed(() => height.value * a)
</script>
