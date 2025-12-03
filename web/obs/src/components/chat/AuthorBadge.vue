<template>
  <yt-live-chat-author-badge-renderer :type="authorTypeText">
    <NTooltip placement="top">
      <div id="image" class="style-scope yt-live-chat-author-badge-renderer">
        <yt-icon
          v-if="isAdmin"
          class="style-scope yt-live-chat-author-badge-renderer">
          <ModeratorIcon />
        </yt-icon>
        <img
          v-else
          :src="`/static/img/icons/guard-level-${privilegeType}.png`"
          class="style-scope yt-live-chat-author-badge-renderer"
          :alt="readableAuthorTypeText" />
      </div>
      <span>{{ readableAuthorTypeText }}</span>
    </NTooltip>
  </yt-live-chat-author-badge-renderer>
</template>

<script lang="ts" setup>
import * as types from './ChatMessageType'

const props = defineProps<{
  isAdmin?: boolean
  privilegeType: types.GuardLevel
}>()

const authorTypeText = computed(() =>
  props.isAdmin ? 'moderator' : props.privilegeType > 0 ? 'member' : '',
)
const readableAuthorTypeText = computed(() =>
  props.isAdmin ? '管理员' : types.GuardLevel.getText(props.privilegeType),
)
</script>

<!-- <style src="@/assets/css/youtube/yt-live-chat-author-badge-renderer.css"></style>
<style src="@/assets/css/youtube/yt-icon.css"></style> -->
