using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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
            if (!isInitialized) 
            {
                label2.Text = "Not Connected";
                return;
            }

            // 測試資料
            string askId = "MainApp";
            string productSeries = "BMS";
            string applicableProjects = "Thai";
            string customizeId = "10000";

            FileStream fileStream = SocketClientAPI.GetBinFileStream(askId, productSeries, applicableProjects, customizeId);
            // 測試用 查看內容是否正確
            using (var reader = new StreamReader(fileStream))
            {
                string content = reader.ReadToEnd();
                label1.Text = content;
            }
            fileStream.Dispose();
        }

        private void CloseConnect_Click(object sender, EventArgs e)
        {
            bool isClose = SocketClientAPI.CloseConnection();
            if (isClose) isInitialized = false;
            label2.Text = "Not Connected";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string stationType = "type";
            string stationName = "name";
            string stationId = "id";
            string operatorId = "operatorId";
            // 初始化連接至Server
            bool isConnecting = SocketClientAPI.InitializeClient(stationType, stationName, stationId, operatorId);

            // 如果沒連接成功 ...
            if (!isConnecting)
            {
                label2.Text = "連接失敗";
                return;
            }

            label2.Text = "連接成功";

            // 測試資料
            string askId = "MainApp";
            string productSeries = "BMS";
            string applicableProjects = "Thai";
            string customizeId = "";
            FileStream fileStream = SocketClientAPI.GetBinFileStream(askId, productSeries, applicableProjects, customizeId);

            // 測試用 查看內容是否正確
            using (var reader = new StreamReader(fileStream))
            {
                string content = reader.ReadToEnd();
                label1.Text = content.Substring(content.Length - 50);
            }

            bool isClose = SocketClientAPI.CloseConnection();
            if (isClose)
            {
                fileStream.Dispose();
                label2.Text = "斷開連接";
            }
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            label2.Text = "Not Connected";
            label1.Text = "";
        }

        private void GetMainApp_Click(object sender, EventArgs e)
        {
            if (!isInitialized)
            {
                label2.Text = "Not Connected";
                return;
            }
            string productSeries = "BMS";
            string applicableProjects = "Thai";
            string customizeId = "10000";

            var mainAppInfo = SocketClientAPI.GetMainApp(productSeries, applicableProjects, customizeId);
            label1.Text = "Version: " + mainAppInfo.Version + "\n" +
                "BLVersion: " + mainAppInfo.BLVersion + "\n" +
                "CalibrationOffset: " + mainAppInfo.CalibrationOffset + "\n";
        }

        private void GetDefaultParam_Click(object sender, EventArgs e)
        {
            if (!isInitialized)
            {
                label2.Text = "Not Connected";
                return;
            }
            string productSeries = "BMS";
            string applicableProjects = "Thai";
            string customizeId = "10000";

            var defaultParaInfo = SocketClientAPI.GetDefaultParameters(productSeries, applicableProjects, customizeId);
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
