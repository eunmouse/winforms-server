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

        private bool connected;
        private NetworkStream stream;

        private static int count; // 사용자수
        public Dictionary<TcpClient, string> clientList = new Dictionary<TcpClient, string>(); // 클라이언트마다 리스트 추가

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
            if (txtIP.InvokeRequired == true)
            {
                txtIP.Invoke((MethodInvoker)(() =>
                {
                    txtIP.ReadOnly = true;
                }));
            }

            if (txtPort.InvokeRequired == true)
            {
                txtPort.Invoke((MethodInvoker)(() =>
                {
                    txtPort.ReadOnly = true;
                }));
            }
        }
        private void Receive()
        {
            try
            {
                while (connected)
                {
                    // 클라이언트의 연결 요청이 오면 TcpClient 반환
                    client = server.AcceptTcpClient();
                    writeRtbChat("클라이언트 연결됨...");
                   
                    // 클라이언트 스트림 값 받아오기
                    stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    // 반환값은 실제로 읽은 바이트 수 (스트림 끝 EOF 도달하면 0 리턴), 연결이 끊어지면 0 반환
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        // 바이트 -> 문자열로 디코딩 
                        string receivedChat = Encoding.Default.GetString(buffer, 0, bytesRead);
                        writeRtbChat("클라이언트 : " + receivedChat);
                    }

                    // TcpClient 닫으면 내부적으로 연결된 NetworkStream 도 닫힘 
                    client.Close();
                    writeRtbChat("클라이언트와의 연결 끊김...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"***받기 실패*** {ex.Message}");
            }
        }

        private void writeRtbChat(string str)
        {
            // rtbChat 객체는 UI 스레드(메인 스레드) 에서만 접근 가능하여, Invoke 로 넘겨서 처리 
            if (rtbChat.InvokeRequired == true)
            {
                rtbChat.Invoke((MethodInvoker)(() =>
                {
                    rtbChat.AppendText(str + Environment.NewLine); // 줄바꿈
                }));
            }
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
            // Write 스레드 생성
            Thread writeThread = new Thread(Write);
            writeThread.IsBackground = true;
            writeThread.Start();
        }

        private void Write()
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

                    stream.Write(byteMsg, 0, byteMsg.Length);
                    writeRtbChat("서버 : " + msg);

                    // txtMessage 객체는 UI 스레드(메인 스레드) 에서만 접근 가능하여, Invoke 로 넘겨서 처리 
                    if (txtMessage.InvokeRequired == true)
                    {
                        txtMessage.Invoke((MethodInvoker)(() =>
                        {
                            txtMessage.ResetText();
                        }));
                    }
                }
            
            }
            catch (Exception ex)
            {
                Console.WriteLine($"***받기 실패*** {ex.Message}");
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
