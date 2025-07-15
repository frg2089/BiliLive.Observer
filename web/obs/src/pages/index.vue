<template>
  <div v-if="!inOBS" text-center text-red text-3xl>
    /// 建议在 OBS 中作为 浏览器停靠窗口使用 ///
  </div>
  <details flex flex-col items-center>
    <summary>OBS 设置</summary>

    <div flex flex-col>
      <label for="host">OBS 主机</label>
      <input
        v-model="obs.config.host"
        type="text"
        name="host"
        px-2
        border
        rounded-lg
        w-full />
      <label for="port">OBS 端口号</label>
      <input
        v-model="obs.config.port"
        type="number"
        name="port"
        px-2
        border
        rounded-lg
        w-full />
      <label for="password">OBS 密码</label>
      <input
        v-model="obs.config.password"
        type="password"
        name="password"
        px-2
        border
        rounded-lg
        w-full />
    </div>

    <button mt-4 px-4 bg-purple rounded-md @click="obs.connect" w-full>
      连接
    </button>
  </details>

  <div v-if="data?.isLogin" flex flex-col items-center>
    <img v-if="avatar" :src="avatar" :alt="data.uname" w-16 h-16 object-cover />
    <div>{{ data.uname }}</div>

    <div grid grid-cols-2 items-center gap-2>
      <button px-4 bg-pink rounded-md @click="start">开始直播</button>
      <button px-4 bg-blue rounded-md @click="stop">停止直播</button>
    </div>

    <details>
      <summary>直播间设置</summary>

      <div>
        <label for="title">直播间标题</label>
        <input
          v-model="liveRoom.title"
          name="title"
          type="text"
          placeholder="请输入房间标题"
          px-2
          border
          rounded-lg
          w-full />
      </div>

      <details>
        <summary>分区设置</summary>
        <details ml-4>
          <summary>大区</summary>

          <div grid grid-cols-3>
            <span v-for="(category, index) in categories" :key="index">
              <input
                type="radio"
                :name="`category-${category.id}`"
                :value="category.id"
                v-model="currentCategoryId" />
              <label :for="`category-${category.id}`">
                {{ category.name }}
              </label>
            </span>
          </div>

          <details v-if="areas?.length" ml-4>
            <summary>小区</summary>

            <div grid grid-cols-3>
              <span v-for="(area, index) in areas" :key="index">
                <input
                  type="radio"
                  :name="`area-${area.id}`"
                  :value="area.id"
                  v-model="liveRoom.areaId" />
                <label :for="`area-${area.id}`">
                  {{ area.name }}
                </label>
              </span>
            </div>
          </details>
        </details>
      </details>

      <button px-4 bg-purple rounded-md @click="update" w-full>更新设置</button>
    </details>
  </div>
  <div v-else>
    <div flex flex-col items-center>
      <div text-2xl>请先登录</div>
      <LoginQRCode @success="init" />
    </div>
  </div>
</template>

<script lang="ts" setup>
import type { components } from '../../obj/apis'
import { client } from '../api'
import { useOBS } from '../stores/obs'

const inOBS = !!window?.obsstudio?.pluginVersion
const obs = useOBS()

const data = ref<components['schemas']['PersonData']>()
const avatar = ref()
const liveRoom = reactive({
  roomId: 0,
  areaId: 0,
  title: '',
})
const categories = ref<Array<components['schemas']['LiveAreaCategory']>>([])
const currentCategoryId = ref(0)
const areas = computed(
  () => categories.value.find(x => x.id === currentCategoryId.value)?.list,
)

const updateAvatar = async () => {
  if (!data.value?.isLogin) return

  const res = await client.GET('/bili/avatar', {
    parseAs: 'blob',
  })
  if (!res.data) return

  avatar.value = URL.createObjectURL(res.data)
}
const updateRoomInfoByUserId = async () => {
  if (!data.value?.mid) return
  const res = await client.GET('/bili/live/infoByUid', {
    params: {
      query: {
        userId: data.value.mid,
      },
    },
  })
  if (!res.data?.roomid) return
  liveRoom.roomId = res.data.roomid
  liveRoom.title = res.data.title
}
const updateRoomInfo = async () => {
  if (!liveRoom.roomId) return
  const res = await client.GET('/bili/live/info', {
    params: {
      query: {
        roomId: liveRoom.roomId,
      },
    },
  })

  if (!res.data) return

  liveRoom.title = res.data.title
  if (res.data.room_id) liveRoom.roomId = res.data.room_id
  if (res.data.parent_area_id) currentCategoryId.value = res.data.parent_area_id
  if (res.data.area_id) liveRoom.areaId = res.data.area_id
}
const updateAreas = async () => {
  const res = await client.GET('/bili/live/areas')
  categories.value = res.data ?? []
}

const init = async () => {
  obs.connect()
  const res = await client.GET('/bili/current')
  data.value = res.data
  if (!data.value?.isLogin) return
  updateAvatar()
  updateAreas()
  updateRoomInfoByUserId().then(updateRoomInfo)
}

const start = async () => {
  const res = await client.POST('/bili/live/start', {
    body: liveRoom,
  })

  if (!res.data?.rtmp) return

  obs.startStream(res.data.rtmp)
}
const update = async () => {
  await client.POST('/bili/live/update', {
    body: {
      platform: null,
      visitId: null,
      addTag: null,
      delTag: null,
      ...liveRoom,
    },
  })
}
const stop = async () => {
  await client.POST('/bili/live/stop', {
    body: liveRoom,
  })

  obs.stopStream()
}

onMounted(init)
</script>
