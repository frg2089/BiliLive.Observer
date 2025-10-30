<template>
  <div mx-4>
    <div>
      <div v-for="item in data">
        <div v-if="item.type === 'Comment'" class="comment-item">
          <img
            class="avatar"
            :src="get(item.userAvatar)"
            :alt="item.userName" />
          <span class="username">{{ item.userName }}</span>
          <span class="comment">{{ item.commentText }}</span>
        </div>
        <div v-else>{{ item }}</div>
      </div>
    </div>

    <div>人气值: {{ hot }}</div>
  </div>
</template>

<script lang="ts" setup>
import { SSE } from 'sse.js'

import { client, pathChecker } from '../../api'
import { useUser } from '../../stores/user'

const route = useRoute()

const hot = ref(0)
const data = reactive<Bili.Live.Danmaku.Event[]>([])
const user = useUser()

const guardLevelMap = {
  3: '舰长',
  2: '提督',
  1: '总督',
}

const get = (url: string) =>
  pathChecker(`/bili/get?url=${encodeURIComponent(url)}`)
const push = (item: Bili.Live.Danmaku.Event) => {
  data.push(item)
  // TODO: 取消此处注释
  // setTimeout(data.shift, 20 * 1000)
}

const init = async () => {
  if (!('roomId' in route.params)) return
  await user.updateUserInfo()

  const sse = new SSE(
    pathChecker(
      `/bili/live/chat/event?roomId=${route.params.roomId}&userId=${user.userId ?? 0}`,
    ),
  )

  sse.addEventListener('hot', (e: CustomEvent & { data: string }) => {
    hot.value = Number(e.data)
  })

  sse.addEventListener('notification', (e: CustomEvent & { data: string }) => {
    const item = JSON.parse(e.data) as any

    switch (item.cmd) {
      case 'LIVE':
        push({
          type: 'LiveStart',
          roomID: item.roomid,
        })
        break
      case 'PREPARING':
        push({
          type: 'LiveEnd',
          roomID: item.roomid,
        })
        break
      case 'DANMU_MSG':
        push({
          type: 'Comment',
          commentText: item.info[1],
          userID: item.info[2][0],
          userAvatar: item.info[0][15].user.base.face,
          userName: item.info[2][1],
          isAdmin: item.info[2][2] === '1',
          isVIP: item.info[2][3] === '1',
          userGuardLevel: item.info[7],
        })
        break
      case 'SEND_GIFT':
        push({
          type: 'GiftSend',
          giftName: item.data.giftName,
          userName: item.data.uname,
          userID: item.data.uid,
          giftCount: item.data.num,
        })
        break
      case 'GIFT_TOP':
        push({
          type: 'GiftTop',
          giftRanking: item.data.map((v: any) => ({
            uid: v.uid,
            userName: v.uname,
            coin: v.coin,
          })),
        })
        break
      case 'WELCOME':
        push({
          type: 'Welcome',
          userName: item.data.uname,
          userID: item.data.uid,
          isVIP: true,
          isAdmin: item.data.isadmin === '1',
        })
        break
      case 'WELCOME_GUARD':
        push({
          type: 'WelcomeGuard',
          userName: item.data.username,
          userID: item.data.uid,
          userGuardLevel: item.data.guard_level,
        })
        break
      case 'GUARD_BUY':
        push({
          type: 'GuardBuy',
          userID: item.data.uid,
          userName: item.data.username,
          userGuardLevel: item.data.guard_level,
          giftName:
            guardLevelMap[
              item.data.guard_level as keyof typeof guardLevelMap
            ] ?? '',
          giftCount: item.data.num,
        })
        break
      case 'SUPER_CHAT_MESSAGE':
      case 'SUPER_CHAT_MESSAGE_JP':
        push({
          type: 'SuperChat',
          commentText: item.data.message,
          userID: item.data.uid,
          userName: item.data.user_info.uname,
          price: item.data.price,
          scKeepTime: item.data.time,
        })
        break
      case 'INTERACT_WORD':
        push({
          type: 'Interact',
          userName: item.data.uname,
          userID: item.data.uid,
          interactType: item.data.msg_type,
        })
        break
      case 'WARNING':
        push({
          type: 'Warning',
          commentText: item.msg,
        })
        break
      case 'CUT_OFF':
        push({
          type: 'LiveEnd',
          commentText: item.msg,
        })
        break
      case 'WATCHED_CHANGE':
        push({
          type: 'WatchedChange',
          watchedCount: item.data.num,
        })
        break
      default:
        if (item.cmd && item.cmd.startsWith('DANMU_MSG')) {
          push({
            type: 'Comment',
            commentText: item.info[1],
            userID: item.info[2][0],
            userAvatar: item.info[0][15].user.base.face,
            userName: item.info[2][1],
            isAdmin: item.info[2][2] === '1',
            isVIP: item.info[2][3] === '1',
            userGuardLevel: item.info[7],
          })
        }
        break
    }
  })
}
onMounted(init)
</script>
<style lang="scss">
.comment-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;

  .avatar {
    display: inline-block;
    width: 1rem;
    height: 1rem;
    border-radius: 100%;
  }

  .username {
    color: green;

    &::after {
      content: ': ';
    }
  }
}
</style>
