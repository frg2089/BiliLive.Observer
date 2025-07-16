<template>
  <NForm :model="obs.config" size="small" mx-auto px-2>
    <div relative mb-4 w-full>
      <h2 absolute top-0 my-2 w-full text-center text-lg>OBS 设置</h2>
      <RouterLink to="/">
        <NButton type="primary" text> 返回首页 </NButton>
      </RouterLink>
    </div>
    <NFormItem
      :rule="[{ required: true, message: '请输入OBS主机地址' }]"
      path="host"
      label="主机地址">
      <NInput
        v-model:value="obs.config.host"
        type="text"
        placeholder="请输入OBS主机地址"
        clearable />
    </NFormItem>
    <NFormItem
      :rule="[{ required: true, message: '请输入OBS主机端口号' }]"
      path="port"
      label="端口号">
      <NInputNumber
        v-model:value="obs.config.port"
        type="text"
        placeholder="请输入OBS端口号"
        w-full />
    </NFormItem>
    <NFormItem path="password" label="连接密码">
      <NInput
        v-model:value="obs.config.password"
        type="password"
        placeholder="请输入OBS密码" />
    </NFormItem>
    <div flex flex-col gap-2>
      <NButton
        :loading="obs.connecting"
        :disabled="obs.connected"
        type="primary"
        size="tiny"
        @click="obs.connect"
        w-full>
        <span v-if="obs.connected">已连接</span>
        <span v-else>连接</span>
      </NButton>
      <NButton
        :loading="obs.connecting"
        :disabled="!obs.connected"
        type="error"
        size="tiny"
        @click="obs.disconnect"
        w-full>
        <span v-if="obs.connected">断开连接</span>
        <span v-else>已断开</span>
      </NButton>
    </div>
  </NForm>
</template>

<script lang="ts" setup>
import { NButton } from 'naive-ui'

import { useOBS } from '../stores/obs'

const obs = useOBS()
</script>
