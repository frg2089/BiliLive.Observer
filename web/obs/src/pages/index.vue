<template>
  <div v-if="!inOBS" text-center text-red text-3xl>
    /// 建议在 OBS 中作为 浏览器停靠窗口使用 ///
  </div>
  <div v-if="!obs.connected" text-center px-4 text-lg>
    我们发现您还未连接至 OBS，请
    <RouterLink to="/obs">
      <NButton type="primary" text-lg text> 前往设置 </NButton>
    </RouterLink>
    。
  </div>
  <NSpin :show="loading">
    <div v-if="data" mx-auto px-2>
      <Avatar mx-auto :face="data.face" :frame="data.pendant.image_enhance" />
      <h2 text-center text-lg>
        {{ data.uname }}
      </h2>
      <div flex flex-col justify-center gap-2 my-2>
        <NButton
          :loading="btnLoading"
          type="primary"
          size="tiny"
          @click="start">
          开始直播
        </NButton>
        <NButton :loading="btnLoading" size="tiny" @click="stop">
          停止直播
        </NButton>
      </div>

      <NCollapse>
        <NCollapseItem title="直播间设置" name="1">
          <NForm :model="liveRoom" size="small" mx-auto px-4>
            <NFormItem
              label="直播间名称"
              path="title"
              :rule="[{ required: true, message: '请输入直播间名称' }]">
              <NInput
                v-model:value="liveRoom.title"
                placeholder="请输入直播间名称" />
            </NFormItem>
            <NFormItem
              label="直播间分区"
              path="areaId"
              :rule="[{ required: true, message: '请选择直播分区' }]">
              <NCascader
                v-model:value="liveRoom.areaId"
                :options
                placeholder="请选择直播间分区"
                expand-trigger="hover"
                check-strategy="child" />
            </NFormItem>
            <div flex flex-col justify-center gap-2 my-2>
              <NButton
                :loading="btnLoading"
                type="primary"
                size="tiny"
                @click="update">
                更新设置
              </NButton>
              <RouterLink to="/obs">
                <NButton type="primary" size="tiny" text w-full>
                  前往 OBS 设置
                </NButton>
              </RouterLink>
              <RouterLink to="/login">
                <NButton type="error" size="tiny" text w-full>
                  返回登录页
                </NButton>
              </RouterLink>
            </div>
          </NForm>
        </NCollapseItem>
      </NCollapse>
    </div>
  </NSpin>
</template>

<script lang="ts" setup>
import type { CascaderOption } from 'naive-ui'

import type { components } from '../../obj/apis'
import { client } from '../api'
import { useOBS } from '../stores/obs'

const inOBS = !!window?.obsstudio?.pluginVersion
const obs = useOBS()
const router = useRouter()
const loading = ref(false)
const btnLoading = ref(false)

const data = ref<components['schemas']['PersonData']>()
const liveRoom = reactive({
  roomId: 0,
  areaId: 0,
  title: '',
})

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
  if (res.data.area_id) liveRoom.areaId = res.data.area_id
}

const options = ref<CascaderOption[]>([])
const updateAreas = async () => {
  const res = await client.GET('/bili/live/areas')
  if (!res.data?.length) return
  options.value = res.data.map(
    i =>
      ({
        label: i.name,
        value: Number(i.id),
        children: i.list.map(
          i =>
            ({
              label: i.name,
              value: Number(i.id),
            }) as CascaderOption,
        ),
      }) as CascaderOption,
  )
}

const init = async () => {
  obs.connect()
  loading.value = true
  try {
    const res = await client.GET('/bili/current')
    data.value = res.data
    if (!data.value?.isLogin) return
    await updateAreas()
    await updateRoomInfoByUserId()
    await updateRoomInfo()
  } finally {
    loading.value = false
  }
}

const start = async () => {
  btnLoading.value = true
  try {
    const res = await client.POST('/bili/live/start', {
      body: liveRoom,
    })

    if (res.error?.title === '60024') {
      if ('qr' in res.error) {
        router.push({
          name: '/face-auth',
          query: {
            qr: res.error.qr as string,
          },
        })
        return
      }
    }

    if (!res.data?.rtmp) return

    await obs.startStream(res.data.rtmp)
  } finally {
    btnLoading.value = false
  }
}
const update = async () => {
  btnLoading.value = true
  try {
    await client.POST('/bili/live/update', {
      body: {
        platform: null,
        visitId: null,
        addTag: null,
        delTag: null,
        ...liveRoom,
      },
    })
  } finally {
    btnLoading.value = false
  }
}
const stop = async () => {
  btnLoading.value = true
  try {
    await client.POST('/bili/live/stop', {
      body: liveRoom,
    })

    await obs.stopStream()
  } finally {
    btnLoading.value = false
  }
}

onMounted(init)
</script>
