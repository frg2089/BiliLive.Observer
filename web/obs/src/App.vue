<template>
  <NConfigProvider :theme>
    <RouterView />
  </NConfigProvider>
</template>

<script lang="ts" setup>
import { darkTheme, useOsTheme } from 'naive-ui'

const osTheme = useOsTheme()
const theme = computed(() => (osTheme.value === 'dark' ? darkTheme : null))
</script>
