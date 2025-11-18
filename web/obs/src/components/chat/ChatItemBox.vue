<template>
  <TextMessage
    v-if="message.type === types.MessageType.TEXT"
    class="style-scope yt-live-chat-item-list-renderer"
    :time="message.time"
    :avatarUrl="message.avatarUrl"
    :authorName="message.authorName"
    :authorType="message.authorType"
    :privilegeType="message.privilegeType"
    :contentParts="getShowContentParts(message)" />
  <PaidMessage
    v-else-if="message.type === types.MessageType.GIFT"
    class="style-scope yt-live-chat-item-list-renderer"
    :time="message.time"
    :avatarUrl="message.avatarUrl"
    :authorName="getShowAuthorName(message)"
    :price="message.price"
    :priceText="message.price <= 0 ? getGiftShowNameAndNum(message) : ''"
    :content="
      message.price <= 0 ? '' : getGiftShowContent(message, showGiftName)
    " />
  <MembershipItem
    v-else-if="message.type === types.MessageType.MEMBER"
    class="style-scope yt-live-chat-item-list-renderer"
    :time="message.time"
    :avatarUrl="message.avatarUrl"
    :authorName="getShowAuthorName(message)"
    :privilegeType="message.privilegeType"
    :title="message.title" />
  <PaidMessage
    v-else-if="message.type === types.MessageType.SUPER_CHAT"
    class="style-scope yt-live-chat-item-list-renderer"
    :time="message.time"
    :avatarUrl="message.avatarUrl"
    :authorName="getShowAuthorName(message)"
    :price="message.price"
    :priceText="''"
    :content="getShowContent(message)" />
</template>

<script lang="ts" setup>
import * as types from '../../types/ChatMessageType'
import {
  getGiftShowContent,
  getGiftShowNameAndNum,
  getShowAuthorName,
  getShowContent,
  getShowContentParts,
} from './constants'
import MembershipItem from './MembershipItem.vue'
import PaidMessage from './PaidMessage.vue'
import TextMessage from './TextMessage.vue'

withDefaults(
  defineProps<{
    message: types.AnyDisplayMessage
    showGiftName: boolean
  }>(),
  {
    showGiftName: false,
  },
)
</script>
