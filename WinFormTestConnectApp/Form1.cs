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
        public static extern IntPtr GetBinFileInfo(string askId, string productSeries, string applicableProjects);

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
                    label2.Text = "Not Connected";
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
            label2.Text = "Not Connected";
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
    }
}
