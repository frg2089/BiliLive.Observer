<template>
  <cropper-canvas
    ref="cropperCanvas"

    background
    :theme-color="theme.primaryColor">
    <cropper-image
      ref="cropperImage"
      :src="image"
      alt="Picture"
      initial-center-size="cover"
      rotatable
      scalable
      skewable
      translatable
      @transform="onImageTransform" />
    <cropper-shade
      hidden
      :theme-color="theme.modalColor"
      style="opacity: 50%" />
    <cropper-handle action="move" plain />
    <cropper-selection
      ref="cropperSelection"
      id="cropperSelection"
      :aspectRatio="16 / 9"
      :initial-coverage="0.8"
      movable
      resizable
      @change="onSelectionChange">
      <cropper-grid role="grid" covered />
      <cropper-crosshair centered />
      <cropper-handle action="move" :theme-color="theme.actionColor" />
      <cropper-handle action="n-resize" :theme-color="theme.primaryColor" />
      <cropper-handle action="e-resize" :theme-color="theme.primaryColor" />
      <cropper-handle action="s-resize" :theme-color="theme.primaryColor" />
      <cropper-handle action="w-resize" :theme-color="theme.primaryColor" />
      <cropper-handle action="ne-resize" :theme-color="theme.primaryColor" />
      <cropper-handle action="nw-resize" :theme-color="theme.primaryColor" />
      <cropper-handle action="se-resize" :theme-color="theme.primaryColor" />
      <cropper-handle action="sw-resize" :theme-color="theme.primaryColor" />
    </cropper-selection>
  </cropper-canvas>
</template>

<script lang="ts" setup>
import CropperCanvas from '@cropper/element-canvas'
import CropperCrosshair from '@cropper/element-crosshair'
import CropperGrid from '@cropper/element-grid'
import CropperHandle from '@cropper/element-handle'
import CropperImage from '@cropper/element-image'
import CropperSelection from '@cropper/element-selection'
import type { Selection } from '@cropper/element-selection'
import CropperShade from '@cropper/element-shade'
import CropperViewer from '@cropper/element-viewer'
import { useThemeVars } from 'naive-ui'

CropperCanvas.$define()
CropperImage.$define()
CropperHandle.$define()
CropperShade.$define()
CropperSelection.$define()
CropperGrid.$define()
CropperCrosshair.$define()
CropperViewer.$define()

const props = defineProps<{
  image: string
  inBounds?: boolean
}>()

const theme = useThemeVars()

const cropperCanvas = ref<CropperCanvas>()
const cropperImage = ref<CropperImage>()
const cropperSelection = ref<CropperSelection>()

const inSelection = (selection: Selection, maxSelection: Selection) => {
  return (
    selection.x >= maxSelection.x &&
    selection.y >= maxSelection.y &&
    selection.x + selection.width <= maxSelection.x + maxSelection.width &&
    selection.y + selection.height <= maxSelection.y + maxSelection.height
  )
}
const onImageTransform = (event: CustomEvent) => {
  if (!props.inBounds) return
  if (!cropperCanvas.value) return
  if (!cropperImage.value) return
  if (!cropperSelection.value) return

  const cropperCanvasRect = cropperCanvas.value.getBoundingClientRect()

  // 1. Clone the cropper image.
  const cropperImageClone = cropperImage.value.cloneNode() as CropperImage

  // 2. Apply the new matrix to the cropper image clone.
  cropperImageClone.style.transform = `matrix(${event.detail.matrix.join(', ')})`

  // 3. Make the cropper image clone invisible.
  cropperImageClone.style.opacity = '0'

  // 4. Append the cropper image clone to the cropper canvas.
  cropperCanvas.value.appendChild(cropperImageClone)

  // 5. Compute the boundaries of the cropper image clone.
  const cropperImageRect = cropperImageClone.getBoundingClientRect()

  // 6. Remove the cropper image clone.
  cropperCanvas.value.removeChild(cropperImageClone)

  const selection = cropperSelection.value as Selection
  const maxSelection: Selection = {
    x: cropperImageRect.left - cropperCanvasRect.left,
    y: cropperImageRect.top - cropperCanvasRect.top,
    width: cropperImageRect.width,
    height: cropperImageRect.height,
  }

  if (!inSelection(selection, maxSelection)) {
    event.preventDefault()
  }
}
const onSelectionChange = (event: CustomEvent) => {
  if (!props.inBounds) return
  if (!cropperCanvas.value) return
  if (!cropperImage.value) return
  if (!cropperSelection.value) return

  const cropperCanvasRect = cropperCanvas.value.getBoundingClientRect()

  const cropperImageRect = cropperImage.value.getBoundingClientRect()
  const selection = event.detail as Selection
  const maxSelection: Selection = {
    x: cropperImageRect.left - cropperCanvasRect.left,
    y: cropperImageRect.top - cropperCanvasRect.top,
    width: cropperImageRect.width,
    height: cropperImageRect.height,
  }

  if (!inSelection(selection, maxSelection)) {
    event.preventDefault()
  }
}
</script>
