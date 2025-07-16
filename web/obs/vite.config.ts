import vue from '@vitejs/plugin-vue'
import UnoCSS from 'unocss/vite'
import AutoImport from 'unplugin-auto-import/vite'
import { NaiveUiResolver } from 'unplugin-vue-components/resolvers'
import Components from 'unplugin-vue-components/vite'
import Router from 'unplugin-vue-router/vite'
import { defineConfig } from 'vite'

export default defineConfig({
  plugins: [
    AutoImport({
      imports: [
        'vue',
        'vue-router',
        {
          'naive-ui': [
            'useDialog',
            'useMessage',
            'useNotification',
            'useLoadingBar',
          ],
        },
      ],
      dts: './obj/auto-imports.d.ts',
    }),
    Components({
      dts: './obj/components.d.ts',
      resolvers: [NaiveUiResolver()],
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
