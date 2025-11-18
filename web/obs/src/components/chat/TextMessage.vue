<template>
  <yt-live-chat-text-message-renderer
    :author-type="authorTypeText"
    :blc-guard-level="privilegeType">
    <ImgShadow
      id="author-photo"
      height="24"
      width="24"
      class="style-scope yt-live-chat-text-message-renderer"
      :imgUrl="avatarUrl" />
    <div id="content" class="style-scope yt-live-chat-text-message-renderer">
      <span
        id="timestamp"
        class="style-scope yt-live-chat-text-message-renderer">
        {{ timeText }}
      </span>
      <AuthorChip
        class="style-scope yt-live-chat-text-message-renderer"
        :isInMemberMessage="false"
        :authorName="authorName"
        :authorType="authorType"
        :privilegeType="privilegeType" />
      <span id="message" class="style-scope yt-live-chat-text-message-renderer">
        <template v-for="(content, index) in contentParts" :key="index">
          <!-- 如果CSS设置的尺寸比属性设置的尺寸还大，在图片加载完后布局会变化，可能导致滚动卡住，没什么好的解决方法 -->
          <img
            v-if="content.type === types.ContentPartType.IMAGE"
            class="emoji yt-formatted-string style-scope yt-live-chat-text-message-renderer"
            :src="content.url"
            :alt="content.text"
            :shared-tooltip-text="content.text"
            :id="`emoji-${content.text}`"
            :width="content.width"
            :height="content.height"
            :class="{ 'blc-large-emoji': content.height >= 100 }" />
          <span v-else-if="content.type === types.ContentPartType.TEXT">
            {{ content.text }}
          </span>
        </template>
      </span>
    </div>
  </yt-live-chat-text-message-renderer>
</template>

<script lang="ts" setup>
// import * as utils from '@/utils'

import * as types from '../../types/ChatMessageType'
import AuthorChip from './AuthorChip.vue'
import ImgShadow from './ImgShadow.vue'

const props = defineProps<{
  avatarUrl: string
  time: Date
  authorName: string
  authorType: types.AuthorType
  contentParts: Array<types.AnyContentPart>
  privilegeType: types.GuardLevel
}>()

const timeText = computed(() => getTimeTextHourMin(props.time))
const authorTypeText = computed(() => types.AuthorTypeText[props.authorType])
</script>

<!-- <style src="@/assets/css/youtube/yt-live-chat-text-message-renderer.css"></style> -->
