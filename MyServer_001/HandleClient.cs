using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyServer_001
{
    internal class HandleClient
    {
        private frmServer form;

        // 클라이언트 소켓 
        private TcpClient client = null;
        private Dictionary<TcpClient, string> clientList = null;

        private NetworkStream stream = null;
        private string userName = null;

        // 생성자로 frmServer 객체를 받아옴
        public HandleClient(frmServer frmServer)
        {
            form = frmServer;
        }

        public void startClient(TcpClient client, Dictionary<TcpClient, string> clientList)
        {
            this.client = client;
            this.clientList = clientList;

            // 클라이언트 스트림 값 받아오기
            stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytes = stream.Read(buffer, 0, buffer.Length);

            // 바이트 -> 문자열로 디코딩 
            string userName = Encoding.Default.GetString(buffer, 0, bytes);

            // 클라이언트 추가 
            clientList.Add(client, userName);
            form.writeRtbChat("[" + userName + "] " + "님이 입장하셨습니다.");

            // 작업스레드 생성
            Thread handleThread = new Thread(doChat);
            handleThread.IsBackground = true;
            handleThread.Start();
        }

        public void doChat()
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            // stream.Read() 는 클라이언트가 메시지 보낼 때까지 대기
            // stream.Read() 는 실제로 읽은 바이트 수 반환 (스트림 끝 EOF 도달하면 0 리턴), 연결이 끊어지면 0 반환
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                // 바이트 -> 문자열로 디코딩 
                string receivedChat = Encoding.Default.GetString(buffer, 0, bytesRead);
                string[] parts = receivedChat.Split('|');
                userName = parts[0];
                string userMsg = parts[1];
                form.writeRtbChat(userName + " : " + userMsg);
            }

            // TcpClient 닫으면 내부적으로 연결된 NetworkStream 도 닫힘 
            client.Close();
            form.writeRtbChat("[" + userName + "] " + "님이 퇴장하셨습니다.");
        }
    }
}
