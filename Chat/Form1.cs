using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat
{
    public partial class Form1 : Form
    {
        private UdpClient udpClient;
        private CancellationTokenSource receiveCancellation;

        public Form1()
        {
            InitializeComponent();
            button2.Enabled = false; 
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                IPAddress multicastIp = IPAddress.Parse("224.5.5.11");
                int localPort = 8001;

                udpClient = new UdpClient(localPort);
                udpClient.JoinMulticastGroup(multicastIp);

                
                button1.Enabled = false; 
                button2.Enabled = true;

                receiveCancellation = new CancellationTokenSource();
                await Task.Run(() => ReceiveMessages(receiveCancellation.Token));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            receiveCancellation?.Cancel();
            udpClient.DropMulticastGroup(IPAddress.Parse("224.5.5.11"));
            udpClient.Close();

            button1.Enabled = true;
            button2.Enabled = false;
            MessageBox.Show("Вы покинули чат", "Аут", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private async Task ReceiveMessages(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync();

                    string receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                    AppendMessage(receivedMessage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string name = textBox4.Text;
                string message = $"{name}: {textBox5.Text}";

                IPAddress multicastIp = IPAddress.Parse("224.5.5.11");
                int localPort = 8001;

                byte[] data = Encoding.UTF8.GetBytes(message);
                await udpClient.SendAsync(data, data.Length, new IPEndPoint(multicastIp, localPort));

                textBox5.Text = ""; 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AppendMessage(string message)
        {
            if (textBox3.InvokeRequired)
            {
                textBox3.Invoke(new Action<string>(AppendMessage), message);
            }
            else
            {
                textBox3.Text += message;
            }
        }
    }
}