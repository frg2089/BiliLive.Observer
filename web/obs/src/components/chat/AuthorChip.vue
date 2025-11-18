<template>
  <yt-live-chat-author-chip>
    <span
      id="author-name"
      dir="auto"
      class="style-scope yt-live-chat-author-chip"
      :class="{ member: isInMemberMessage }"
      :type="authorTypeText">
      <!-- why there was <template> before? -->
      <span>{{ authorName }}</span>
      <!-- 这里是已验证勋章 -->
      <span
        id="chip-badges"
        class="style-scope yt-live-chat-author-chip"></span>
    </span>
    <span id="chat-badges" class="style-scope yt-live-chat-author-chip">
      <AuthorBadge
        v-if="isInMemberMessage"
        class="style-scope yt-live-chat-author-chip"
        :isAdmin="false"
        :privilegeType="privilegeType" />
      <template v-else>
        <AuthorBadge
          v-if="authorType === types.AuthorType.ADMIN"
          class="style-scope yt-live-chat-author-chip"
          isAdmin
          :privilegeType="0" />
        <AuthorBadge
          v-if="privilegeType > 0"
          class="style-scope yt-live-chat-author-chip"
          :isAdmin="false"
          :privilegeType="privilegeType" />
      </template>
    </span>
  </yt-live-chat-author-chip>
</template>

<script lang="ts" setup>
import * as types from '../../types/ChatMessageType'
import AuthorBadge from './AuthorBadge.vue'

const props = defineProps<{
  isInMemberMessage: boolean
  authorName: string
  authorType: types.AuthorType
  privilegeType: types.GuardLevel
}>()

const authorTypeText = computed(() => types.AuthorTypeText[props.authorType])
</script>

<!-- <style src="@/assets/css/youtube/yt-live-chat-author-chip.css"></style> -->
