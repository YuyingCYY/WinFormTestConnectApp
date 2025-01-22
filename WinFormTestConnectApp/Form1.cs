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

            try
            {
                if (isInitialized)
                {
                    FileStream fileStream = SocketClientAPI.GetBinFileStream(askId, productSeries, applicableProjects, customizeId);
                    try
                    {
                        // 測試用 查看內容是否正確
                        using (var reader = new StreamReader(fileStream))
                        {
                            string content = reader.ReadToEnd();
                            label1.Text = content;
                        }
                    }
                    finally
                    {
                        fileStream.Dispose();
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
            bool isClose = SocketClientAPI.CloseConnection();
            if (isClose) isInitialized = false;
            label2.Text = "Not Connected";
        }
    }
}
