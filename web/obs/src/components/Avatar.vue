<template>
  <div relative w-16 h-16 flex items-center justify-center>
    <img
      block
      w-9
      h-9
      rounded-full
      :src="path(`/bili/get?url=${encodeURIComponent(face)}`)" />
    <img
      v-if="frame"
      absolute
      left-0
      top-0
      w-16
      h-16
      block
      z-1
      :src="path(`/bili/get?url=${encodeURIComponent(frame)}`)" />
  </div>
</template>

<script lang="ts" setup>
import { pathChecker } from '../api'

defineProps<{
  face: string
  frame?: string
}>()

const path = pathChecker
</script>
