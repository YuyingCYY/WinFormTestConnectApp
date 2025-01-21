using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinFormTestConnectApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        bool isInitialized = false;

        private void InitializeConnect(object sender, EventArgs e)
        {
            if (isInitialized) return;

            string stationType = "type";
            string stationName = "name";
            string stationId = "id";
            string operatorId = "operatorId";

            // 初始化連接至Server
            isInitialized = SocketClientAPI.InitializeClient(stationType, stationName, stationId, operatorId);

            if (isInitialized) label2.Text = "Connecting";
        }

        private void GetFile_Click(object sender, EventArgs e)
        {
            // 測試資料
            string askId = "MainApp";
            string productSeries = "BMS";
            string applicableProjects = "Thai";
            string customizeId = "10000";

            FileStream fileStream;

            try
            {
                if (isInitialized)
                {
                    IntPtr fileInfoPtr = SocketClientAPI.GetBinFileInfo(askId, productSeries, applicableProjects, customizeId);
                    if (fileInfoPtr == IntPtr.Zero)
                    {
                        throw new Exception("Failed to receive file");
                    }
                    try
                    {
                        // 將 Ptr 轉換成 FileInfo 結構
                        SocketClientAPI.FileInfo fileInfo = Marshal.PtrToStructure<SocketClientAPI.FileInfo>(fileInfoPtr);
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
                        SocketClientAPI.FreeFileInfo(fileInfoPtr);
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
            SocketClientAPI.CloseConnection();
            isInitialized = false;
            label2.Text = "Not Connected";
        }
    }
}
