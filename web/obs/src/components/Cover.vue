<template>
  <div mx-auto relative flex items-center justify-center>
    <NUpload
      ref="upload"
      accept="image/*"
      name="file"
      directory-dnd
      :show-file-list="false"
      :action="path('/bili/live/cover')"
      :max="1"
      @finish="finish"
      @error="error">
      <NUploadDragger p-0>
        <img
          block
          object-cover
          :src="path(`/bili/get?url=${encodeURIComponent(cover)}`)" />
      </NUploadDragger>
    </NUpload>
  </div>
</template>

<script lang="ts" setup>
import type { NUpload, UploadOnFinish } from 'naive-ui'
import type { OnError } from 'naive-ui/es/upload/src/interface'

import { pathChecker } from '../api'

defineProps<{
  cover: string
}>()

const emit = defineEmits<{
  (e: 'refresh'): void
}>()

const message = useMessage()
const upload = ref<InstanceType<typeof NUpload>>()

const path = pathChecker

const finish: UploadOnFinish = ({ file }) => {
  message.success('封面上传成功，将在审核通过后展示')
  upload.value?.clear()
  emit('refresh')
  return file
}

const error: OnError = ({ event }) => {
  const res = JSON.parse((event?.target as XMLHttpRequest).response)
  message.error(res.detail)
  upload.value?.clear()
}
</script>
