import { darkTheme, useOsTheme } from 'naive-ui'
import { defineStore } from 'pinia'

export type Theme = 'auto' | 'light' | 'dark'
export type ThemeValue = Exclude<Theme, 'auto'>

export const useTheme = defineStore('theme', () => {
  const osTheme = useOsTheme()
  const current = ref<Theme>('auto')
  const computedValue = computed(() => {
    if (current.value === 'auto')
      return osTheme.value === 'dark' ? 'dark' : 'light'

    return current.value
  })

  const naiveTheme = computed(() =>
    computedValue.value === 'dark' ? darkTheme : null,
  )

  const switchTheme = () => {
    if (current.value === 'auto') current.value = 'light'
    else if (current.value === 'light') current.value = 'dark'
    else current.value = 'auto'
  }
  return {
    current,
    computed: computedValue,
    naiveTheme,
    switchTheme,
  }
})
