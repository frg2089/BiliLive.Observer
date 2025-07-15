import vue from '@vitejs/plugin-vue'
import UnoCSS from 'unocss/vite'
import AutoImport from 'unplugin-auto-import/vite'
import Components from 'unplugin-vue-components/vite'
import Router from 'unplugin-vue-router/vite'
import { defineConfig } from 'vite'

export default defineConfig({
  plugins: [
    AutoImport({
      imports: ['vue'],
      dts: './obj/auto-imports.d.ts',
    }),
    Components({
      dts: './obj/components.d.ts',
    }),
    Router({
      dts: './obj/typed-router.d.ts',
    }),
    UnoCSS(),
    vue({
      template: {
        compilerOptions: {
          isCustomElement: tag => tag.includes('-'),
        },
      },
    }),
  ],
  server: {
    proxy: {
      '/bili': {
        target: 'https://localhost:7073',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
