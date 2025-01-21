using System.Runtime.InteropServices;
using System;

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
        public static extern void CloseConnection();

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
    }
}
