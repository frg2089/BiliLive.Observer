import OBSWebSocket from 'obs-websocket-js'
import { defineStore } from 'pinia'

import type { components } from '../../obj/apis'

export const useOBS = defineStore(
  'obs',
  () => {
    const client = new OBSWebSocket()
    const config = reactive({
      host: 'localhost',
      port: 4455,
      password: '',
    })
    const url = computed(() => `ws://${config.host}:${config.port}`)

    const connect = async () => {
      await client.disconnect()

      await client.connect(url.value, config.password)
    }

    const startStream = async (rtmp: components['schemas']['RTMP']) => {
      client.call('SetStreamServiceSettings', {
        streamServiceType: 'rtmp_custom',
        streamServiceSettings: {
          server: rtmp.addr,
          key: rtmp.code,
        },
      })

      client.call('StartStream')
    }

    const stopStream = async () => {
      client.call('StopStream')
    }

    return {
      client,
      config,
      url,
      connect,
      startStream,
      stopStream,
    }
  },
  {
    persist: {
      pick: ['config'],
    },
  },
)
