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
    const connecting = ref(false)
    const connected = ref(false)

    client.on('ConnectionOpened', () => (connected.value = true))
    client.on('ConnectionClosed', () => (connected.value = false))

    const connect = async () => {
      connecting.value = true
      try {
        await client.connect(url.value, config.password)
      } finally {
        connecting.value = false
      }
    }

    const disconnect = async () => {
      connecting.value = true
      try {
        await client.disconnect()
      } finally {
        connecting.value = false
      }
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
      connecting,
      connected,
      connect,
      disconnect,
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
