#if HE_SYSCORE

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace HeathenEngineering.UX
{
    /// <summary>
    /// Structure for the custom Heathen Log.
    /// </summary>
    [Serializable]
    public class LogData
    {
        [Serializable]
        public class Entry
        {
            public long time;
            public string type;
            public string message;
            public string stack;

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("Time:\t");
                sb.Append(DateTime.FromBinary(time).ToString(API.Log.dateTimeFormat));
                sb.AppendLine();

                sb.Append("Type:\t");
                sb.Append(type);
                sb.AppendLine();

                sb.Append("Message:\n\t");
                sb.Append(message);
                sb.AppendLine();

                if (API.Log.logStackTrace && !string.IsNullOrEmpty(stack))
                {
                    sb.Append("Stack Trace:\n\t");
                    sb.Append(stack);
                    sb.AppendLine();
                }

                return sb.ToString();
            }
        }

        public string companyName;
        public string productName;
        public string productVersion;
        public string engineVersion;
        public string commandLine;
        public string platform;
        public string deviceType;
        public string deviceModel;
        public string deviceName;
        public string os;
        public string osFamily;
        public string cpuType;
        public int cpuCount;
        public int cpuFrequency;
        public int systemMemory;
        public string gpuVendor;
        public string gpuType;
        public string gpuName;
        public string gpuVersion;
        public int gpuMemory;
        public int gpuShaderLevel;
        public List<Entry> entries = new List<Entry>();

        public LogData()
        {
            companyName = Application.companyName;
            productName = Application.productName;
            productVersion = Application.version;
            engineVersion = Application.unityVersion;
            commandLine = CommandLine.GetArgumentLine();
            platform = Application.platform.ToString();
            deviceType = SystemInfo.deviceType.ToString();
            deviceModel = SystemInfo.deviceModel;
            deviceName = SystemInfo.deviceName;
            os = SystemInfo.operatingSystem;
            osFamily = SystemInfo.operatingSystemFamily.ToString();
            cpuType = SystemInfo.processorType;
            cpuCount = SystemInfo.processorCount;
            cpuFrequency = SystemInfo.processorFrequency;
            systemMemory = SystemInfo.systemMemorySize;
            gpuVendor = SystemInfo.graphicsDeviceVendor;
            gpuType = SystemInfo.graphicsDeviceType.ToString();
            gpuName = SystemInfo.graphicsDeviceName;
            gpuVersion = SystemInfo.graphicsDeviceVersion;
            gpuMemory = SystemInfo.graphicsMemorySize;
            gpuShaderLevel = SystemInfo.graphicsShaderLevel;
        }

        public void Add(string message, string stack, LogType type)
        {
            entries.Add(new Entry()
            {
                message = message,
                stack = stack,
                type = type.ToString(),
                time = DateTime.Now.ToBinary(),
            });
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Company Name:\t\t");
            sb.Append(companyName);
            sb.AppendLine();

            sb.Append("Product Name:\t\t");
            sb.Append(productName);
            sb.AppendLine();

            sb.Append("Product Version:\t");
            sb.Append(productVersion);
            sb.AppendLine();

            sb.Append("Engine Version:\t\t");
            sb.Append(engineVersion);
            sb.AppendLine();

            sb.Append("Command Line:\t\t");
            sb.Append(commandLine);
            sb.AppendLine();

            sb.Append("Platform:\t\t");
            sb.Append(platform);
            sb.AppendLine();

            sb.Append("Device Type:\t\t");
            sb.Append(deviceType);
            sb.AppendLine();

            sb.Append("Device Model:\t\t");
            sb.Append(deviceModel);
            sb.AppendLine();

            sb.Append("Device Name:\t\t");
            sb.Append(deviceName);
            sb.AppendLine();

            sb.Append("OS:\t\t\t");
            sb.Append(os);
            sb.AppendLine();

            sb.Append("OS Family:\t\t");
            sb.Append(osFamily);
            sb.AppendLine();

            sb.Append("CPU Type:\t\t");
            sb.Append(cpuType);
            sb.AppendLine();

            sb.Append("CPU Frequency:\t\t");
            sb.Append(cpuFrequency);
            sb.AppendLine();

            sb.Append("System Memory:\t\t");
            sb.Append(systemMemory);
            sb.AppendLine();

            sb.Append("GPU Vendor:\t\t");
            sb.Append(gpuVendor);
            sb.AppendLine();

            sb.Append("GPU Type:\t\t");
            sb.Append(gpuType);
            sb.AppendLine();

            sb.Append("GPU Name:\t\t");
            sb.Append(gpuName);
            sb.AppendLine();

            sb.Append("GPU Version:\t\t");
            sb.Append(gpuVersion);
            sb.AppendLine();

            sb.Append("GPU Memory:\t\t");
            sb.Append(gpuMemory);
            sb.AppendLine();

            sb.Append("Shader Level:\t\t");
            sb.Append(gpuShaderLevel);
            sb.AppendLine();

            sb.AppendLine();
            sb.Append("Messages:\t\t");
            sb.Append(entries.Count);
            sb.AppendLine();

            foreach (var entry in entries)
            {
                sb.Append(entry.ToString());
                sb.AppendLine();
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        /// <summary>
        /// Saves the current log to a JSON formated file
        /// </summary>
        /// <param name="fileName">The name of the file to save</param>
        /// <param name="path">The path to save the file to</param>
        /// <returns>The full path of the saved file</returns>
        public string SaveJson(string fileName, string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = Application.persistentDataPath;

            if (!fileName.EndsWith(".json", System.StringComparison.InvariantCultureIgnoreCase))
                fileName += ".json";

            if (path.EndsWith("/") || path.EndsWith("\\"))
                path = path.Substring(0, path.Length - 1);

            if (fileName.StartsWith("/") || fileName.StartsWith("\\"))
                fileName = fileName.Substring(1);

            System.IO.File.WriteAllText(path + "/" + fileName, this.ToJson(), Encoding.UTF8);
            var fileInfo = new System.IO.FileInfo(path + "/" + fileName);
            return fileInfo.FullName;
        }
        /// <summary>
        /// Saves the current log to a human friendly formated text file
        /// </summary>
        /// <param name="fileName">The name of the file to save</param>
        /// <param name="path">The path to save the file to</param>
        /// <returns>The full path of the saved file</returns>
        public string SaveText(string fileName, string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = Application.persistentDataPath;

            if (!fileName.EndsWith(".txt", System.StringComparison.InvariantCultureIgnoreCase))
                fileName += ".txt";

            if (path.EndsWith("/") || path.EndsWith("\\"))
                path = path.Substring(0, path.Length - 1);

            if (fileName.StartsWith("/") || fileName.StartsWith("\\"))
                fileName = fileName.Substring(1);

            System.IO.File.WriteAllText(path + "/" + fileName, this.ToString(), Encoding.UTF8);
            var fileInfo = new System.IO.FileInfo(path + "/" + fileName);
            return fileInfo.FullName;
        }
    }
}

#endif
