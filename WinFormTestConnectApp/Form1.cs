using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Linq;

namespace WinFormTestConnectApp
{
    public partial class Form1 : Form
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct FileInfo
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
        public static extern void CloseConnection();

        /// <summary>
        /// 釋放資源
        /// </summary>
        /// <param name="fileInfo"></param>
        [DllImport("SocketClient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeFileInfo(IntPtr fileInfo);

        [DllImport("SocketClient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SendData(string askId, string productSeries, string applicableProjects, bool isGetFile = false);

        [DllImport("SocketClient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ReceiveData(byte[] buffer, int bufferSize);

        /// <summary>
        /// 獲取.bin檔 IntPtr 轉換成 FileInfo 結構
        /// </summary>
        /// <param name="askId">站點類型</param>
        /// <param name="productSeries">產品系列</param>
        /// <param name="applicableProjects">適用專案</param>
        [DllImport("SocketClient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetBinFileInfo(string askId, string productSeries, string applicableProjects);

        [DllImport("SocketClient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetMainAppInfo(string productSeries, string applicableProjects);

        [DllImport("SocketClient.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetDefaultParametersInfo(string productSeries, string applicableProjects);

        public Form1()
        {
            InitializeComponent();
        }

        bool isInitialized = false;

        private void GetFile_Click(object sender, EventArgs e)
        {
            // 測試資料
            string askId = "MainApp";
            string productSeries = "BMS";
            string applicableProjects = "Thai";

            FileStream fileStream = null;

            try
            {
                if (isInitialized)
                {
                    IntPtr fileInfoPtr = GetBinFileInfo(askId, productSeries, applicableProjects);
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

                        // 返回檔案的FileStream，使用FileMode.Open確保檔案存在
                        fileStream = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose);

                        // 測試用 查看內容是否正確
                        using (var reader = new StreamReader(fileStream))
                        {
                            string content = reader.ReadToEnd();
                            label1.Text = content;
                        }
                    }
                    finally
                    {
                        FreeFileInfo(fileInfoPtr);
                    }
                }
                else
                {
                    label2.Text = "Not Connecting";
                }
            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
                throw ex;
            }
        }

        private void CloseConnect_Click(object sender, EventArgs e)
        {
            CloseConnection();
            isInitialized = false;
            label2.Text = "Not Connecting";
        }

        private void GetData_Click(object sender, EventArgs e)
        {
            if (!isInitialized)
            {
                label2.Text = "Not Connecting";
                return;
            }
            string askId = "MainApp";
            string productSeries = "BMS";
            string applicableProjects = "Thai";

            int result;
            try
            {
                result = SendData(askId, productSeries, applicableProjects);
                if (result == -1)
                {
                    label1.Text = "Error";
                }

                byte[] buffer = new byte[4096];
                int bytesReceived = ReceiveData(buffer, buffer.Length);
                if (bytesReceived > 0)
                {
                    // 處理接收到的數據
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                    label1.Text = receivedData;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"sError connecting: {ex}");
            }
        }

        private void InitializeConnect(object sender, EventArgs e)
        {
            if (isInitialized) return;

            string stationType = "type";
            string stationName = "name";
            string stationId = "id";
            string operatorId = "operatorId";

            // 初始化連接至Server
            isInitialized = InitializeClient(stationType, stationName, stationId, operatorId);

            if (isInitialized) label2.Text = "Connecting";
        }

        private void GetMainApp_Click(object sender, EventArgs e)
        {
            if (!isInitialized)
            {
                label2.Text = "Not Connecting";
                return;
            }
            string productSeries = "BMS";
            string applicableProjects = "Thai";

            IntPtr mainAppInfoPtr = GetMainAppInfo(productSeries, applicableProjects);
            MainAppInfo mainAppInfo = Marshal.PtrToStructure<MainAppInfo>(mainAppInfoPtr);
            label1.Text = "Version: " + mainAppInfo.Version + "\n" +
                "BLVersion: " + mainAppInfo.BLVersion + "\n" +
                "CalibrationOffset: " + mainAppInfo.CalibrationOffset + "\n";
        }

        private void GetDefaultParameters_Click(object sender, EventArgs e)
        {
            if (!isInitialized)
            {
                label2.Text = "Not Connecting";
                return;
            }
            string productSeries = "BMS";
            string applicableProjects = "Thai";

            IntPtr defaultParaInfoPtr = GetDefaultParametersInfo(productSeries, applicableProjects);
            DefaultParametersInfo defaultParaInfo = Marshal.PtrToStructure<DefaultParametersInfo>(defaultParaInfoPtr);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < defaultParaInfo.ShieldedZoneCount; i++)
            {
                int start = defaultParaInfo.ShieldedZone[i].start;
                int end = defaultParaInfo.ShieldedZone[i].end;
                sb.AppendLine($"Shielded Zone: start={start}, end={end}");
            };

            label1.Text = "Version: " + defaultParaInfo.Version + "\n" +
                "BLVersion: " + defaultParaInfo.BLVersion + "\n" +
                "CalibrationOffset: " + defaultParaInfo.CalibrationOffset + "\n" +
                "ShieldedZoneCount: " + defaultParaInfo.ShieldedZoneCount + "\n" +
                sb.ToString();
        }
    }
}
