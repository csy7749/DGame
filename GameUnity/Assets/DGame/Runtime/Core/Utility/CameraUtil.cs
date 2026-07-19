using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DGame
{
    public static partial class Utility
    {
        public static class CameraUtil
        {
            public static void StackOverlayCamera(Camera baseCamera, Camera overlayCamera)
            {
                if (baseCamera == null)
                {
                    throw new ArgumentNullException(nameof(baseCamera));
                }

                if (overlayCamera == null)
                {
                    throw new ArgumentNullException(nameof(overlayCamera));
                }

                UniversalAdditionalCameraData baseCameraData = baseCamera.GetUniversalAdditionalCameraData();
                UniversalAdditionalCameraData overlayCameraData = overlayCamera.GetUniversalAdditionalCameraData();
                baseCameraData.renderType = CameraRenderType.Base;
                overlayCameraData.renderType = CameraRenderType.Overlay;

                var cameraStack = baseCameraData.cameraStack;
                if (cameraStack == null)
                {
                    throw new InvalidOperationException($"{baseCamera.name} does not support URP camera stacking.");
                }

                cameraStack.Remove(overlayCamera);
                cameraStack.Add(overlayCamera);
            }

            public static void SetCameraRenderTypeBase(Camera camera)
            {
                if (camera == null)
                {
                    throw new ArgumentNullException(nameof(camera));
                }

                camera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Base;
            }
        }
    }
}
