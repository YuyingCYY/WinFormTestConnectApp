using System.Runtime.InteropServices;
using System;
using System.IO;

namespace WinFormTestConnectApp
{
    public class SocketClientAPI
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct FileInfo
        {
            public IntPtr Data;
            public UIntPtr Size;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string FileName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MainAppInfo
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string Version;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string BLVersion;
            public int CalibrationOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ShieldedZoneInfo
        {
            public int start;
            public int end;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DefaultParametersInfo
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string Version;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string BLVersion;

            public int CalibrationOffset;

            public int ShieldedZoneCount;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ShieldedZoneInfo[] ShieldedZone;
        }

        /// <summary>
        /// 初始化客戶端
        /// </summary>
        /// <param name="stationType">本站類型</param>
        /// <param name="stationName">本站名稱</param>
        /// <param name="stationId">本站號碼</param>
        /// <param name="operatorId">登入操作人員</param>
        /// <returns>連接是否成功</returns>
        [DllImport("SocketClient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool InitializeClient(string stationType, string stationName, string stationId, string operatorId);

        /// <summary>
        /// 關閉連接
        /// </summary>
        [DllImport("SocketClient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CloseConnection();

        [DllImport("SocketClient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetMainAppInfo(string productSeries, string applicableProjects, string customizeId);

        [DllImport("SocketClient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetDefaultParametersInfo(string productSeries, string applicableProjects, string customizeId);

        /// <summary>
        /// 釋放資源
        /// </summary>
        /// <param name="fileInfo"></param>
        [DllImport("SocketClient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeFileInfo(IntPtr fileInfo);

        /// <summary>
        /// 獲取.bin檔 IntPtr 轉換成 FileInfo 結構
        /// </summary>
        /// <param name="askId">站點類型</param>
        /// <param name="productSeries">產品系列</param>
        /// <param name="applicableProjects">適用專案</param>
        [DllImport("SocketClient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetBinFileInfo(string askId, string productSeries, string applicableProjects, string customizeId);

        public static MainAppInfo GetMainApp(string productSeries, string applicableProjects, string customizeId)
        {
            IntPtr mainAppInfoPtr = GetMainAppInfo(productSeries, applicableProjects, customizeId);
            MainAppInfo mainAppInfo = Marshal.PtrToStructure<MainAppInfo>(mainAppInfoPtr);
            return mainAppInfo;
        }

        public static DefaultParametersInfo GetDefaultParameters(string productSeries, string applicableProjects, string customizeId)
        {
            IntPtr defaultParamInfoPtr = GetDefaultParametersInfo(productSeries, applicableProjects, customizeId);
            DefaultParametersInfo defaultParamInfo = Marshal.PtrToStructure<DefaultParametersInfo>(defaultParamInfoPtr);
            return defaultParamInfo;
        }

        public static FileStream GetBinFileStream(string askId, string productSeries, string applicableProjects, string customizeId)
        {
            IntPtr fileInfoPtr = GetBinFileInfo(askId, productSeries, applicableProjects, customizeId);
            if (fileInfoPtr == IntPtr.Zero)
            {
                throw new Exception("Failed to receive file");
            }

            try
            {
                // 將 Ptr 轉換成 FileInfo 結構
                FileInfo fileInfo = Marshal.PtrToStructure<FileInfo>(fileInfoPtr);
                byte[] buffer = new byte[(int)fileInfo.Size];
                Marshal.Copy(fileInfo.Data, buffer, 0, (int)fileInfo.Size);

                // 創建臨時檔案
                string tempPath = Path.Combine(Path.GetTempPath(), fileInfo.FileName);
                File.WriteAllBytes(tempPath, buffer);

                // 返回檔案的 FileStream，使用 FileMode.Open 確保檔案存在
                return new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose);
            }
            finally
            {
                FreeFileInfo(fileInfoPtr);
            }
        }
    }
}
