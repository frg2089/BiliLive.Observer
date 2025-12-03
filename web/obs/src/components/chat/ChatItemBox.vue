<template>
  <TextMessage
    v-if="message.type === types.MessageType.TEXT"
    class="style-scope yt-live-chat-item-list-renderer"
    :time="message.time"
    :avatar-url="message.avatarUrl"
    :author-name="message.authorName"
    :author-type="message.authorType"
    :privilege-type="message.privilegeType"
    :content-parts="getShowContentParts(message)" />
  <PaidMessage
    v-else-if="message.type === types.MessageType.GIFT"
    class="style-scope yt-live-chat-item-list-renderer"
    :time="message.time"
    :avatar-url="message.avatarUrl"
    :author-name="getShowAuthorName(message)"
    :price="message.price"
    :price-text="message.price <= 0 ? getGiftShowNameAndNum(message) : ''"
    :content="
      message.price <= 0 ? '' : getGiftShowContent(message, showGiftName)
    " />
  <MembershipItem
    v-else-if="message.type === types.MessageType.MEMBER"
    class="style-scope yt-live-chat-item-list-renderer"
    :time="message.time"
    :avatar-url="message.avatarUrl"
    :author-name="getShowAuthorName(message)"
    :privilege-type="message.privilegeType"
    :title="message.title" />
  <PaidMessage
    v-else-if="message.type === types.MessageType.SUPER_CHAT"
    class="style-scope yt-live-chat-item-list-renderer"
    :time="message.time"
    :avatar-url="message.avatarUrl"
    :author-name="getShowAuthorName(message)"
    :price="message.price"
    :content="getShowContent(message)" />
</template>

<script lang="ts" setup>
import * as types from './ChatMessageType'
import {
  getGiftShowContent,
  getGiftShowNameAndNum,
  getShowAuthorName,
  getShowContent,
  getShowContentParts,
} from './constants'

defineProps<{
  message: types.AnyDisplayMessage
  showGiftName?: boolean
}>()
</script>
