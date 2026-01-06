import { defineStore } from 'pinia'

import type { components } from '../../obj/apis'
import { client } from '../api'

export const useUser = defineStore('user', () => {
  const data = ref<components['schemas']['PersonData']>()
  const areas = ref<Array<components['schemas']['LiveAreaCategory']>>()
  const userId = computed(() => data.value?.mid)

  const updateUserInfo = async () => {
    const res1 = await client.GET('/bili/current')
    data.value = res1.data

    if (data.value?.isLogin !== true) {
      data.value = undefined
    }
  }

  const updateAreas = async () => {
    const res = await client.GET('/bili/live/areas')
    areas.value = res.data
  }

  return {
    data,
    areas,
    userId,
    updateUserInfo,
    updateAreas,
  }
})
