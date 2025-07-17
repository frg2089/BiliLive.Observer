import createClient from 'openapi-fetch'

import type { paths } from '../obj/apis'

export const client = createClient<paths>({
  baseUrl: import.meta.env.VITE_BACKEND_BASE_URL,
})

/**
 * 用于检查 API 合法性的工具
 * @param path
 * @returns
 */
export const pathChecker = (path: keyof paths | `${keyof paths}?${string}`) =>
  path

export const formDataBodySerializer = <T extends object>(data?: T) => {
  const form = new FormData()
  if (!data) return form

  Object.entries(data).forEach(([k, v]) => form.append(k, v))
  return form
}
