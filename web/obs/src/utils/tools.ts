export const delay = (ms: number) =>
  new Promise<void>(resolve => setTimeout(resolve, ms))

export const formatCurrency = (price: number) => {
  return new Intl.NumberFormat('zh-CN', {
    minimumFractionDigits: price < 100 ? 2 : 0,
  }).format(price)
}

export const getTimeTextHourMin = (date: Date) => {
  let hour = date.getHours()
  let min = `00${date.getMinutes()}`.slice(-2)
  return `${hour}:${min}`
}
