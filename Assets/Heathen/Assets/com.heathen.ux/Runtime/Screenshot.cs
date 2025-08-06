#if HE_SYSCORE

using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace HeathenEngineering.UX.API
{
    public static class Screenshot
    {
        public enum ImageType
        {
            JPG,
            PNG,
            TGA,
            EXR
        }

        /// <summary>
        /// Capture the full screen
        /// </summary>
        /// <remarks>
        /// Runs a coroutine silently
        /// </remarks>
        /// <param name="componenet">Any active and enabled MonoBehaviour can be provided here, this is simply used to detect "EndOfFrame"</param>
        /// <param name="callback"><code>{ result, error }</code>Called when the process compeltes</param>
        public static void Capture(MonoBehaviour componenet, Action<Texture2D, bool> callback)
        {
            componenet.StartCoroutine(CaptureCoroutine(default, default, callback));
        }

        /// <summary>
        /// Captures a given area of the screen
        /// </summary>
        /// <remarks>
        /// Runs a coroutine silently
        /// </remarks>
        /// <param name="componenet">Any active and enabled MonoBehaviour can be provided here, this is simply used to detect "EndOfFrame"</param>
        /// <param name="from">Bottom left of the rect of the screen to capture</param>
        /// <param name="to">Top right of the rect of the screen to capture</param>
        /// <param name="callback"><code>{ result, error }</code>Called when the process compeltes</param>
        public static void Capture(MonoBehaviour componenet, Vector2Int from, Vector2Int to, Action<Texture2D, bool> callback)
        {
            componenet.StartCoroutine(CaptureCoroutine(from, to, callback));
        }

        /// <summary>
        /// Captures a given area of the screen
        /// </summary>
        /// <param name="from">Bottom left of the rect of the screen to capture</param>
        /// <param name="to">Top right of the rect of the screen to capture</param>
        /// <param name="callback"><code>{ result, error }</code>Called when the process compeltes</param>
        /// <returns></returns>
        public static IEnumerator CaptureCoroutine(Vector2Int from = default, Vector2Int to = default, Action<Texture2D, bool> callback = null)
        {
            yield return new WaitForEndOfFrame();

            if (callback != null)
            {
                try
                {
                    if (to == default)
                        to = new Vector2Int(Screen.width, Screen.height);

                    if (to.x < 1)
                        to.x = 1;
                    if (to.y < 1)
                        to.y = 1;
                    if (to.x > Screen.width)
                        to.x = Screen.width;
                    if (to.y > Screen.height)
                        to.y = Screen.height;

                    if (from.x < 0)
                        from.x = 0;
                    if (from.y < 0)
                        from.y = 0;
                    if (from.x >= to.x)
                        from.x = to.x - 1;
                    if (from.y >= to.y)
                        from.y = to.y - 1;

                    Texture2D screenImage = new Texture2D(to.x - from.x, to.y - from.y);
                    screenImage.ReadPixels(new Rect(from.x, from.y, to.x, to.y), 0, 0);
                    screenImage.Apply();
                    callback(screenImage, false);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    callback(null, true);
                }
            }
        }

        public static void SaveImage(Texture2D image, string filePath, ImageType type, Action<string, bool> callback)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    throw new InvalidOperationException("File path is empty!");

                switch(type)
                {
                    case ImageType.EXR:
                        byte[] exrBytes = image.EncodeToEXR();
                        System.IO.File.WriteAllBytes(filePath, exrBytes);
                        break;
                    case ImageType.JPG:
                        byte[] jpgBytes = image.EncodeToJPG();
                        System.IO.File.WriteAllBytes(filePath, jpgBytes);
                        break;
                    case ImageType.PNG:
                        byte[] pngBytes = image.EncodeToPNG();
                        System.IO.File.WriteAllBytes(filePath, pngBytes);
                        break;
                    case ImageType.TGA:
                        byte[] tgaBytes = image.EncodeToTGA();
                        System.IO.File.WriteAllBytes(filePath, tgaBytes);
                        break;
                }

                if (callback != null)
                {
                    var fileInfo = new System.IO.FileInfo(filePath);
                    callback?.Invoke(fileInfo.FullName, false);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to save image: " + ex.Message);
                callback?.Invoke(string.Empty, true);
            }
        }

        public static void SaveImage(MonoBehaviour componenet, string filePath = null, ImageType type = ImageType.JPG, Vector2Int resolution = default, Vector2Int from = default, Vector2Int to = default, Action<Texture2D, string, bool> callback = null)
        {
            componenet.StartCoroutine(CaptureCoroutine(from, to, (texture, capFailure) =>
            {
                if(!capFailure)
                {
                    if (resolution.x > 0 && resolution.y > 0)
                        BilinearScale(texture, resolution.x, resolution.y);

                    if (string.IsNullOrEmpty(filePath))
                        filePath = Application.persistentDataPath + "/screenshot_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                    switch(type)
                    {
                        case ImageType.EXR:
                            if (!filePath.EndsWith(".exr", StringComparison.InvariantCultureIgnoreCase))
                                filePath += ".exr";
                            break;
                        case ImageType.JPG:
                            if (!filePath.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase))
                                filePath += ".jpg";
                            break;
                        case ImageType.PNG:
                            if (!filePath.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                                filePath += ".png";
                            break;
                        case ImageType.TGA:
                            if (!filePath.EndsWith(".tga", StringComparison.InvariantCultureIgnoreCase))
                                filePath += ".tga";
                            break;
                    }

                    SaveImage(texture, filePath, type, (path, saveFailure) =>
                    {
                        callback?.Invoke(texture, path, saveFailure);
                    });
                }
                else
                {
                    Debug.LogError("Failed to capture screenshot");
                    callback?.Invoke(texture, string.Empty, true);
                }
            }));
        }

        private class ThreadData
        {
            public int start;
            public int end;
            public ThreadData(int s, int e)
            {
                start = s;
                end = e;
            }
        }

        private static Color32[] texColors;
        private static Color32[] newColors;
        private static int w;
        private static float ratioX;
        private static float ratioY;
        private static int w2;
        private static int finishCount;
        private static Mutex mutex;

        /// <summary>
        /// Perform a point scale adjustment on the indicated texture
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        public static void PointScale(Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight, false);
        }

        /// <summary>
        /// Perform a bilinear scale adjustmentt on the indicated texture
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        public static void BilinearScale(Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight, true);
        }

        private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear)
        {
            texColors = tex.GetPixels32();
            newColors = new Color32[newWidth * newHeight];
            if (useBilinear)
            {
                ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
                ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
            }
            else
            {
                ratioX = ((float)tex.width) / newWidth;
                ratioY = ((float)tex.height) / newHeight;
            }
            w = tex.width;
            w2 = newWidth;
            var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
            var slice = newHeight / cores;

            finishCount = 0;
            if (mutex == null)
            {
                mutex = new Mutex(false);
            }
            if (cores > 1)
            {
                int i = 0;
                ThreadData threadData;
                for (i = 0; i < cores - 1; i++)
                {
                    threadData = new ThreadData(slice * i, slice * (i + 1));
                    ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
                    Thread thread = new Thread(ts);
                    thread.Start(threadData);
                }
                threadData = new ThreadData(slice * i, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
                while (finishCount < cores)
                {
                    Thread.Sleep(1);
                }
            }
            else
            {
                ThreadData threadData = new ThreadData(0, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
            }

            tex.Reinitialize(newWidth, newHeight);
            tex.SetPixels32(newColors);
            tex.Apply();

            texColors = null;
            newColors = null;
        }

        private static void BilinearScale(System.Object obj)
        {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                int yFloor = (int)Mathf.Floor(y * ratioY);
                var y1 = yFloor * w;
                var y2 = (yFloor + 1) * w;
                var yw = y * w2;

                for (var x = 0; x < w2; x++)
                {
                    int xFloor = (int)Mathf.Floor(x * ratioX);
                    var xLerp = x * ratioX - xFloor;
                    newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
                                                           ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp),
                                                           y * ratioY - yFloor);
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        private static void PointScale(System.Object obj)
        {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                var thisY = (int)(ratioY * y) * w;
                var yw = y * w2;
                for (var x = 0; x < w2; x++)
                {
                    newColors[yw + x] = texColors[(int)(thisY + ratioX * x)];
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
        {
            return new Color(c1.r + (c2.r - c1.r) * value,
                              c1.g + (c2.g - c1.g) * value,
                              c1.b + (c2.b - c1.b) * value,
                              c1.a + (c2.a - c1.a) * value);
        }
    }
}

#endif