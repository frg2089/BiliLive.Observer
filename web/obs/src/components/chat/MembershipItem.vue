<template>
  <yt-live-chat-membership-item-renderer
    class="style-scope yt-live-chat-item-list-renderer"
    flex items-center my-2
    show-only-header
    :blc-guard-level="privilegeType">
    <div id="card" class="style-scope yt-live-chat-membership-item-renderer">
      <div
        id="header"
        class="style-scope yt-live-chat-membership-item-renderer">
        <ImgShadow
          id="author-photo"
          height="40"
          width="40"
          class="style-scope yt-live-chat-membership-item-renderer"
          :img-url="avatarUrl" />
        <div
          id="header-content"
          class="style-scope yt-live-chat-membership-item-renderer">
          <div
            id="header-content-primary-column"
            class="style-scope yt-live-chat-membership-item-renderer">
            <div
              id="header-content-inner-column"
              class="style-scope yt-live-chat-membership-item-renderer">
              <AuthorChip
                class="style-scope yt-live-chat-membership-item-renderer"
                is-in-member-message
                :author-name="authorName"
                :author-type="0"
                :privilege-type="privilegeType" />
            </div>
            <div
              id="header-subtext"
              class="style-scope yt-live-chat-membership-item-renderer">
              {{ title }}
            </div>
          </div>
          <div
            id="timestamp"
            class="style-scope yt-live-chat-membership-item-renderer">
            {{ timeText }}
          </div>
        </div>
      </div>
    </div>
  </yt-live-chat-membership-item-renderer>
</template>

<script lang="ts" setup>
// import * as utils from '@/utils'
import * as types from './ChatMessageType'

const props = defineProps<{
  avatarUrl: string
  authorName: string
  privilegeType: types.GuardLevel
  title: string
  time: Date
}>()

const timeText = computed(() => getTimeTextHourMin(props.time))
</script>

<!-- <style src="@/assets/css/youtube/yt-live-chat-membership-item-renderer.css"></style> -->
