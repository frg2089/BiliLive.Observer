<template>
  <div mx-auto relative flex items-center justify-center>
    <NUpload
      ref="upload"
      v-model:file-list="fileList"
      accept="image/*"
      name="file"
      directory-dnd
      :show-file-list="false"
      :max="1"
      :default-upload="false">
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
import type { NUpload, UploadFileInfo, UploadOnFinish } from 'naive-ui'
import type { OnError } from 'naive-ui/es/upload/src/interface'

import { pathChecker } from '../api'

defineProps<{
  cover: string
}>()

const emit = defineEmits<{
  (e: 'refresh'): void
}>()

const router = useRouter()

const fileList = ref<UploadFileInfo[]>([])
watch(
  () => fileList.value,
  v => {
    if (!v.length) return
    const img = v[0]
    if (!img.file) return
    const url = URL.createObjectURL(img.file)
    router.push(`/cover?url=${encodeURIComponent(url)}`)
  },
)

const message = useMessage()
const upload = ref<InstanceType<typeof NUpload>>()

const path = pathChecker
</script>
