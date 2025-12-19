import { defineStore } from 'pinia'

import type { components } from '../../obj/apis'
import { client } from '../api'

export const useUser = defineStore('user', () => {
  const data = ref<components['schemas']['PersonData']>()
  const areas = ref<Array<components['schemas']['LiveAreaCategory']>>()
  const userId = computed(() => data.value?.mid)
  const roomId = ref<number>()

  const updateUserInfo = async () => {
    const res1 = await client.GET('/bili/current')
    data.value = res1.data

    if (data.value?.isLogin !== true) {
      data.value = undefined
    }

    const resRoom = await client.GET('/bili/live/infoByUid', {
      params: {
        query: {
          userId: userId.value!,
        },
      },
    })
    roomId.value = resRoom.data?.roomid ?? undefined
  }

  const updateAreas = async () => {
    const res = await client.GET('/bili/live/areas')
    areas.value = res.data
  }

  return {
    data,
    areas,
    userId,
    roomId,
    updateUserInfo,
    updateAreas,
  }
})
