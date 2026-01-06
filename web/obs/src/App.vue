<template>
  <NConfigProvider :theme="theme.naiveTheme">
    <NMessageProvider>
      <RouterView />
    </NMessageProvider>
  </NConfigProvider>
</template>

<script lang="ts" setup>
import { useTheme } from './stores/theme'

const theme = useTheme()
</script>
