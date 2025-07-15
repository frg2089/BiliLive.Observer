import createClient from 'openapi-fetch'

import type { paths } from '../obj/apis'

export const client = createClient<paths>({
  baseUrl: import.meta.env.VITE_BACKEND_BASE_URL,
})

export const formDataBodySerializer = <T extends object>(data?: T) => {
  const form = new FormData()
  if (!data) return form

  Object.entries(data).forEach(([k, v]) => form.append(k, v))
  return form
}
