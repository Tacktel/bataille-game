using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using NetworkCommsDotNet;
using NetworkCommsDotNet.DPSBase;
using NetworkCommsDotNet.Tools;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.TCP;

namespace Coinche
{
    namespace Client
    {
        public partial class MainWindow : Window
        {
            /// <summary>
            /// MainWindow constructor
            /// </summary>
            public MainWindow()
            {
                InitializeComponent();
                messageText.IsEnabled = false;
                sendMessageButton.IsEnabled = false;
                playerButton.IsEnabled = false;
                aiButton.IsEnabled = false;
                playersListButton.IsEnabled = false;
                putButton.IsEnabled = false;
                richChatBox.IsReadOnly = true;
                disconnectButton.IsEnabled = false;
                inventoryButton.IsEnabled = false;
                AppendLineToRichChatBox("Welcome to the Bataille game ! Please fill the server fields and click on \"Connect\"...");
                localName.Text = HostInfo.HostName;
                serverIP.Text = "127.0.0.1";
                NetworkComms.AppendGlobalIncomingPacketHandler<Message>("Message", HandleIncomingMessage);
                NetworkComms.AppendGlobalConnectionCloseHandler(HandleConnectionClosed);
                //FlowDocument myFlowDoc = new FlowDocument();
            }

            #region Private Fields
            Dictionary<ShortGuid, Message> lastPeerMessageDict = new Dictionary<ShortGuid, Message>();
            int relayMaximum = 3;
            long messageSendIndex = 0;
            string username;
            ConnectionInfo serverConnectionInfo = null;
            #endregion

            /// <summary>
            /// Add line to chat message box
            /// </summary>
            /// <param name="message">message to print</param>
            private void AppendLineToRichChatBox(string message)
            {
                richChatBox.Dispatcher.BeginInvoke(new Action<string>((messageToAdd) =>
                {
                    richChatBox.AppendText(messageToAdd + "\r");
                    richChatBox.ScrollToEnd();
                }), new object[] { message });
            }

            /// <summary>
            /// click handler to connect to a specific server
            /// </summary>
            private void ConnectToServer_Click(object sender, RoutedEventArgs e)
            {
                if (serverIP.Text != "")
                {
                    try
                    {
                        serverConnectionInfo = new ConnectionInfo(serverIP.Text.Trim(), int.Parse(serverPort.Text));
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Failed to parse the server IP and port. Please ensure it is correct and try again", "Server IP & Port Parse Error", MessageBoxButton.OK);
                        return;
                    }
                }

                Message messageToSend = new Message(NetworkComms.NetworkIdentifier, localName.Text, "CONNECT", "", messageSendIndex++);
                if (serverConnectionInfo != null)
                {
                    try
                    {
                        TCPConnection.GetConnection(serverConnectionInfo).SendObject("Message", messageToSend);
                    }
                    catch (CommsException)
                    {
                        MessageBox.Show("A CommsException occurred while trying to send message to " + serverConnectionInfo, "CommsException", MessageBoxButton.OK);
                        return;
                    }
                    messageText.IsEnabled = true;
                    sendMessageButton.IsEnabled = true;
                    playerButton.IsEnabled = true;
                    aiButton.IsEnabled = true;
                    playersListButton.IsEnabled = true;
                    connectButton.IsEnabled = false;
                    disconnectButton.IsEnabled = true;
                    serverIP.IsEnabled = false;
                    serverPort.IsEnabled = false;
                    username = localName.Text;
                    localName.IsEnabled = false;
                }
            }

            /// <summary>
            /// Send specific command to the server
            /// </summary>
            /// <param name="command">the command to send</param>
            private void SendCommand(string command)
            {
                Message messageToSend = new Message(NetworkComms.NetworkIdentifier, username, command, "", messageSendIndex++);
                try
                {
                    TCPConnection.GetConnection(serverConnectionInfo).SendObject("Message", messageToSend);
                }
                catch (CommsException)
                {
                    MessageBox.Show("A CommsException occurred while trying to send message to " + serverConnectionInfo, "CommsException", MessageBoxButton.OK);
                    return;
                }
            }

            /// <summary>
            /// click handler to retrieve other players in the server
            /// </summary>
            private void GetPlayersList_Click(object sender, RoutedEventArgs e)
            {
                SendCommand("LIST");
            }
            /// <summary>
            /// Click handler to send message through the chat
            /// </summary>
            private void SendMessageButton_Click(object sender, RoutedEventArgs e)
            {
                SendMessage();
            }
            /// <summary>
            /// Key handler to send message in the chat
            /// </summary>
            private void MessageText_KeyUp(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Enter || e.Key == Key.Return)
                    SendMessage();
            }
            /// <summary>
            /// callback called when window is closed
            /// </summary>
            private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            {
                NetworkComms.Shutdown();
            }
            /// <summary>
            /// Disable user connection to server when its full
            /// </summary>
            public void HandleServerFull()
            {
                Action disableButtons = delegate ()
                {
                    sendMessageButton.IsEnabled = false;
                    messageText.IsEnabled = false;
                    aiButton.IsEnabled = false;
                    playerButton.IsEnabled = false;
                };
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, disableButtons);
            }
            /// <summary>
            /// Deal with user cards hand
            /// </summary>
            /// <param name="usernameInMessage">user to handle</param>
            private void HandleHand(string usernameInMessage)
            {
                if (usernameInMessage != username) return;
                AppendLineToRichChatBox("It's your turn, pick a card !");
                Action enablePutButton = delegate ()
                {
                    putButton.IsEnabled = true;
                };
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, enablePutButton);
            }
            /// <summary>
            /// Handle end of game
            /// </summary>
            private void HandleEnd()
            {
                Action disablePutButton = delegate ()
                {
                    putButton.IsEnabled = false;
                };
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, disablePutButton);
            }
            /// <summary>
            /// Handle commands that are sent by the different clients
            /// </summary>
            /// <param name="incomingMessage">the message sent by clients</param>
            private void HandleSpecificMessage(Message incomingMessage)
            {
                switch (incomingMessage.coincheHeader)
                {
                    case "SERVER_FULL":
                        HandleServerFull();
                        break;
                    case "GAME":
                        break;
                    case "HAND":
                        HandleHand(incomingMessage.MessageContent);
                        break;
                    case "END":
                        HandleEnd();
                        break;
                    default:
                        break;
                }
            }
            /// <summary>
            /// Deal with incomming message (print or subprocess)
            /// </summary>
            /// <param name="header">packet header</param>
            /// <param name="connection">connection</param>
            /// <param name="incomingMessage">message sent</param>
            private void HandleIncomingMessage(PacketHeader header, Connection connection, Message incomingMessage)
            {
                lock (lastPeerMessageDict)
                {
                    if (lastPeerMessageDict.ContainsKey(incomingMessage.SourceIdentifier))
                    {
                        if (lastPeerMessageDict[incomingMessage.SourceIdentifier].MessageIndex < incomingMessage.MessageIndex)
                        {
                            if (incomingMessage.coincheHeader == "CHAT")
                            {
                                AppendLineToRichChatBox("[" + (DateTime.Now).ToString("HH:mm:ss dd/MM/yyyy") + "] " + incomingMessage.SourceName + " : " + incomingMessage.MessageContent);
                                lastPeerMessageDict[incomingMessage.SourceIdentifier] = incomingMessage;
                            }
                        }
                        if (incomingMessage.coincheHeader != "CHAT")
                            HandleSpecificMessage(incomingMessage);
                    }
                    else
                    {
                        if (incomingMessage.coincheHeader == "CHAT")
                        {
                            lastPeerMessageDict.Add(incomingMessage.SourceIdentifier, incomingMessage);
                            AppendLineToRichChatBox("[" + (DateTime.Now).ToString("HH:mm:ss dd/MM/yyyy") + "] " + incomingMessage.SourceName + " : " + incomingMessage.MessageContent);
                        }
                        else
                            HandleSpecificMessage(incomingMessage);
                    }
                }

                if (incomingMessage.RelayCount < relayMaximum)
                {
                    var allRelayConnections = (from current in NetworkComms.GetExistingConnection() where current != connection select current).ToArray();

                    incomingMessage.IncrementRelayCount();

                    foreach (var relayConnection in allRelayConnections)
                    {
                        try { relayConnection.SendObject("Message", incomingMessage); }
                        catch (CommsException) { /* Catch the comms exception, ignore and continue */ }
                    }
                }
            }
            /// <summary>
            /// callback to be called when a user reset its connection
            /// </summary>
            /// <param name="connection">connection that is reset</param>
            private void HandleConnectionClosed(Connection connection)
            {
                lock (lastPeerMessageDict)
                {
                    ShortGuid remoteIdentifier = connection.ConnectionInfo.NetworkIdentifier;
                    if (lastPeerMessageDict.ContainsKey(remoteIdentifier))
                        AppendLineToRichChatBox("Connection with '" + lastPeerMessageDict[remoteIdentifier].SourceName + "' has been closed.");
                    else
                        AppendLineToRichChatBox("Connection with '" + connection.ToString() + "' has been closed.");

                    lastPeerMessageDict.Remove(connection.ConnectionInfo.NetworkIdentifier);
                }
            }
            /// <summary>
            /// Send message to clients
            /// </summary>
            private void SendMessage()
            {
                if (messageText.Text.Trim() == "")
                    return;
                Message messageToSend = new Message(NetworkComms.NetworkIdentifier, username, "CHAT", messageText.Text, messageSendIndex++);
                lock (lastPeerMessageDict) lastPeerMessageDict[NetworkComms.NetworkIdentifier] = messageToSend;
                AppendLineToRichChatBox("[" + (DateTime.Now).ToString("HH:mm:ss dd/MM/yyyy") + "] " + messageToSend.SourceName + " : " + messageToSend.MessageContent);
                this.messageText.Text = "";

                if (serverConnectionInfo != null)
                {
                    try
                    {
                        TCPConnection.GetConnection(serverConnectionInfo).SendObject("Message", messageToSend);
                    }
                    catch (CommsException)
                    {
                        MessageBox.Show("A CommsException occurred while trying to send message to " + serverConnectionInfo, "CommsException", MessageBoxButton.OK);
                    }
                }

                var otherConnectionInfos = (from current in NetworkComms.AllConnectionInfo() where current != serverConnectionInfo select current).ToArray();
                foreach (ConnectionInfo info in otherConnectionInfos)
                {
                    try
                    {
                        TCPConnection.GetConnection(info).SendObject("Message", messageToSend);
                    }
                    catch (CommsException)
                    {
                        MessageBox.Show("A CommsException occurred while trying to send message to " + info, "CommsException", MessageBoxButton.OK);
                    }
                }
            }
            /// <summary>
            /// click handler to begin a game with other playesr
            /// </summary>
            private void playerButton_Click(object sender, RoutedEventArgs e)
            {
                SendCommand("PLAYER_PLAY");
                playerButton.IsEnabled = false;
                aiButton.IsEnabled = false;
                inventoryButton.IsEnabled = true;
            }
            /// <summary>
            /// click handler to disconnect from server
            /// </summary>
            private void DisconnectButton_Click(object sender, RoutedEventArgs e)
            {
                Connection co = TCPConnection.GetConnection(serverConnectionInfo);
                co.CloseConnection(true);

                sendMessageButton.IsEnabled = false;
                messageText.IsEnabled = false;
                playerButton.IsEnabled = false;
                aiButton.IsEnabled = false;
                playersListButton.IsEnabled = false;
                putButton.IsEnabled = false;
                richChatBox.IsReadOnly = true;
                disconnectButton.IsEnabled = false;
                connectButton.IsEnabled = true;
                serverIP.IsEnabled = true;
                serverPort.IsEnabled = true;
                localName.IsEnabled = true;
                inventoryButton.IsEnabled = false;
            }
            /// <summary>
            /// click handler to pick a card
            /// </summary>
            private void putButton_Click(object sender, RoutedEventArgs e)
            {
                SendCommand("PUTTED");
                putButton.IsEnabled = false;
            }
            /// <summary>
            /// click handler to get the number of cards in player deck
            /// </summary>
            private void inventoryButton_Click(object sender, RoutedEventArgs e)
            {
                SendCommand("INVENTORY");
            }
            /// <summary>
            /// click handler to begin a game with an ai
            /// </summary>
            private void aiButton_Click(object sender, RoutedEventArgs e)
            {
                SendCommand("AI_PLAY");
                
                playerButton.IsEnabled = false;
                aiButton.IsEnabled = false;
                inventoryButton.IsEnabled = true;
                
            }
        }
    }
}