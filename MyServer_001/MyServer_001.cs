using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MyServer_001
{
    public partial class frmServer : Form
    {
        private TcpListener server; // 서버 소켓 
        private TcpClient client; // 클라이언트 소켓 
        private Dictionary<TcpClient, string> clientList = new Dictionary<TcpClient, string>();

        private HandleClient handleClient;

        private bool connected;
        private NetworkStream stream;

        public frmServer()
        {
            InitializeComponent();
        }

        private void Listen()
        {
            // 서버 소켓은 처음 1회만 연결
            try
            {
                int portNum = Convert.ToInt32(txtPort.Text);
                server = new TcpListener(IPAddress.Parse(txtIP.Text), portNum);
                server.Start();
                writeRtbChat("서버 준비... 클라이언트 기다리는 중...");
                connected = true;
                MakeTxtReadOnly();

                // Receive 스레드 생성 
                Thread receiveThread = new Thread(Receive);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"***연결 실패*** {ex.Message}");
            }
        }
        // 연결 이후 IP 주소 / 포트번호 비활성화 
        private void MakeTxtReadOnly()
        {
            txtIP.SafeInvoke(() => txtIP.ReadOnly = true);
            txtPort.SafeInvoke(() => txtPort.ReadOnly = true);
        }
        private void Receive()
        {
            try
            {
                while (connected)
                {
                    // 접속대기, 클라이언트 연결 요청이 오면 TcpClient 반환
                    client = server.AcceptTcpClient();

                    handleClient = new HandleClient(this);
                    handleClient.startClient(client, clientList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"***받기 실패*** {ex.Message}");
            }
        }

        // 접근제한자 public 으로 설정하여, 다른 클래스에서도 접근 가능하도록 수정 
        public void writeRtbChat(string str)
        {
            // rtbChat 객체는 UI 스레드(메인 스레드) 에서만 접근 가능하여, Invoke 로 넘겨서 처리 
            rtbChat.SafeInvoke(() => rtbChat.AppendText(str + Environment.NewLine)); // 줄바꿈
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 연결 버튼, 메인 스레드와 독립적으로 실행되는 작업 스레드 생성
            Thread listenThread = new Thread(Listen);
            listenThread.IsBackground = true;
            listenThread.Start();
        }

        private void frmServer_Load(object sender, EventArgs e)
        {
            txtIP.Text = "127.0.0.1";
            txtPort.Text = "5003";
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            // Send 스레드 생성
            Thread writeThread = new Thread(Send);
            writeThread.IsBackground = true;
            writeThread.Start();
        }

        private void Send()
        {
            try
            {
                // 클라이언트에게 메시지 전송 
                string msg = txtMessage.Text.Trim();

                // 공백이 아닌 경우에만 메시지 전송되도록 수정 
                if (msg != "")
                {
                    // 문자열 -> 바이트로 인코딩 
                    byte[] byteMsg = Encoding.Default.GetBytes(msg);

                    stream = client.GetStream();
                    stream.Write(byteMsg, 0, byteMsg.Length);
                    writeRtbChat("서버 : " + msg);

                    // txtMessage 객체는 UI 스레드(메인 스레드) 에서만 접근 가능하여, Invoke 로 넘겨서 처리 
                    txtMessage.SafeInvoke(() => txtMessage.ResetText());
                }
            
            }
            catch (Exception ex)
            {
                Console.WriteLine($"***전송 실패*** {ex.Message}");
                writeRtbChat("서버 메세지 전송 실패");
            }
            
        }

        private void frmServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            connected = false;
            client.Close(); // TCP 연결 닫음, stream 도 같이 끊김
        }
    }
}
