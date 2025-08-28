using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace MyServer_001
{
    public partial class frmServer : Form
    {
        private TcpListener server;
        private TcpClient client;

        public frmServer()
        {
            InitializeComponent();
        }

        private void Listen()
        {
            // 서버 연결
            try
            {
                int portNum = Convert.ToInt32(txtPort.Text);
                server = new TcpListener(IPAddress.Parse(txtIP.Text), portNum);
                server.Start();
                writeRtbChat("서버 준비... 클라이언트 기다리는 중...");

                // 클라이언트의 연결 요청이 오면 TcpClient 반환
                client = server.AcceptTcpClient();
                writeRtbChat("클라이언트 연결됨...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"***연결 실패*** {ex.Message}");
            }
        }
        
        private void writeRtbChat(string str)
        {
            // rtbChat 객체는 UI 스레드(메인 스레드) 에서만 접근 가능하여, Invoke 로 넘겨서 처리 
            if (rtbChat.InvokeRequired == true)
            {
                rtbChat.Invoke((MethodInvoker)(() =>
                {
                    rtbChat.Text = str + Environment.NewLine; // 줄바꿈
                }));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 메인 스레드와 독립적으로 실행되는 작업 스레드 생성
            Thread listenThread = new Thread(new ThreadStart(Listen));
            listenThread.IsBackground = true;
            listenThread.Start();
        }

        private void frmServer_Load(object sender, EventArgs e)
        {
            txtIP.Text = "127.1.1.0";
            txtPort.Text = "5003";
        }
    }
}
