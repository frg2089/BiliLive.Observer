<template>
  <yt-img-shadow
    class="no-transition"
    :height="height"
    :width="width"
    style="background-color: transparent"
    loaded>
    <img
      id="img"
      class="style-scope yt-img-shadow"
      alt=""
      :height="height"
      :width="width"
      :src="path(`/bili/get?url=${encodeURIComponent(showImgUrl)}`)"
      @error="onLoadError" />
  </yt-img-shadow>
</template>

<script lang="ts" setup>
import { pathChecker } from '../../api';
import * as constants from './constants'

const props = defineProps<{
  imgUrl: string // => showImgUrl
  height: string
  width: string
}>()

const showImgUrl = ref(props.imgUrl)
watch(
  () => props.imgUrl,
  v => (showImgUrl.value = v),
)
const onLoadError = () => {
  if (showImgUrl.value === constants.DEFAULT_AVATAR_URL) return

  showImgUrl.value = constants.DEFAULT_AVATAR_URL
}

const path = pathChecker
</script>

<!-- <style src="@/assets/css/youtube/yt-img-shadow.css"></style> -->
