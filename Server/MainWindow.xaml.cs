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

using System.Diagnostics;

namespace Coinche
{
    namespace Server
    {
        public partial class MainWindow : Window
        {
            #region Private Fields
            Dictionary<ShortGuid, Message> lastPeerMessageDict = new Dictionary<ShortGuid, Message>();
            int relayMaximum = 3;
            long messageSendIndex = 0;
            List<string> players = new List<string>();
            string[] playersArray;
            Card[] cardsInRound;
            Deck deck;
            int actualPlayerIndex = 0;
            bool gameReady = false;
            bool aiGame = false;
            const string hourFormat = "HH:mm:ss dd/MM/yyyy";
            #endregion

            /// <summary>
            /// Server main window constructor
            /// </summary>
            public MainWindow()
            {
                InitializeComponent();
                AppendLineToRichChatBox("Welcome to the Coinche server...");
                localName.Text = "Server";
                richChatBox.IsReadOnly = true;
                NetworkComms.AppendGlobalIncomingPacketHandler<Message>("Message", HandleIncomingMessage);
                NetworkComms.AppendGlobalConnectionCloseHandler(HandleConnectionClosed);
                Connection.StartListening(ConnectionType.TCP, new IPEndPoint(IPAddress.Any, 0));
                AppendLineToRichChatBox("Listening for incoming TCP connections on:");
                foreach (IPEndPoint listenEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
                    AppendLineToRichChatBox(listenEndPoint.Address + ":" + listenEndPoint.Port);
            }
            /// <summary>
            /// Add line to server chatbox
            /// </summary>
            private void AppendLineToRichChatBox(string message)
            {
                richChatBox.Dispatcher.BeginInvoke(new Action<string>((messageToAdd) =>
                {
                    richChatBox.AppendText(messageToAdd + "\r");
                    richChatBox.ScrollToEnd();
                }), new object[] { message });
            }
            /// <summary>
            /// Refresh chatbox
            /// </summary>
            private void RefreshMessagesFromBox()
            {
                lock (lastPeerMessageDict)
                {
                    string[] currentUsers = (from current in lastPeerMessageDict.Values orderby current.SourceName select current.SourceName).ToArray();
                    this.connectedClientsBox.Dispatcher.BeginInvoke(new Action<string[]>((users) =>
                    {
                        connectedClientsBox.Text = "";
                        foreach (var username in users)
                            if (username != this.localName.Text)
                                connectedClientsBox.AppendText(username + "\n");
                    }), new object[] { currentUsers });
                }
            }
            /// <summary>
            /// click handler to send a message
            /// </summary>
            private void SendMessageButton_Click(object sender, RoutedEventArgs e)
            {
                SendMessage();
            }
            /// <summary>
            /// keyboard handler to send a message to the chat
            /// </summary>
            private void MessageText_KeyUp(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Enter || e.Key == Key.Return)
                    SendMessage();
            }
            /// <summary>
            /// callback for window closing
            /// </summary>
            private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            {
                NetworkComms.Shutdown();
            }
            /// <summary>
            /// add incomming message on chat and relay it
            /// </summary>
            private void AppendToChat(Connection connection, Message incomingMessage)
            {
                if (lastPeerMessageDict.ContainsKey(incomingMessage.SourceIdentifier))
                {
                    if (lastPeerMessageDict[incomingMessage.SourceIdentifier].MessageIndex < incomingMessage.MessageIndex)
                    {
                        AppendLineToRichChatBox("[" + (DateTime.Now).ToString(hourFormat) + "] " + incomingMessage.SourceName + " : " + incomingMessage.MessageContent);
                        lastPeerMessageDict[incomingMessage.SourceIdentifier] = incomingMessage;
                    }
                }
                else
                {
                    lastPeerMessageDict.Add(incomingMessage.SourceIdentifier, incomingMessage);
                    AppendLineToRichChatBox("[" + (DateTime.Now).ToString(hourFormat) + "] " + incomingMessage.SourceName + " : " + incomingMessage.MessageContent);
                }
                if (incomingMessage.RelayCount < relayMaximum)
                {
                    var allRelayConnections = (from current in NetworkComms.GetExistingConnection() where current != connection select current).ToArray();
                    incomingMessage.IncrementRelayCount();
                    foreach (var relayConnection in allRelayConnections)
                    {
                        try
                        { relayConnection.SendObject("Message", incomingMessage); }
                        catch (CommsException)
                        { /* Catch the comms exception, ignore and continue */ }
                    }
                }
            }
            /// <summary>
            /// Accept new client
            /// </summary>
            private void AppendNewClient(Connection connection, Message incomingMessage)
            {
                if (gameReady == false)
                {
                    lastPeerMessageDict.Add(incomingMessage.SourceIdentifier, incomingMessage);
                    BroadcastMessage(connection, incomingMessage, incomingMessage.SourceName + " joined the Bataille !", "Welcome !");
                }
                else
                    BroadcastMessage(connection, incomingMessage, "", "Game has already begun !", "SERVER_FULL");
            }
            /// <summary>
            /// Send players list to a specific one
            /// </summary>
            private void SendClientsList(Connection connection, Message incomingMessage)
            {
                string clientsList = "[ ";
                lock (lastPeerMessageDict)
                {
                    string[] currentUsers = (from current in lastPeerMessageDict.Values orderby current.SourceName select current.SourceName).ToArray();
                    this.connectedClientsBox.Dispatcher.BeginInvoke(new Action<string[]>((users) =>
                    {
                        foreach (var username in users)
                            if (username != this.localName.Text && username != incomingMessage.SourceName)
                                clientsList += username + ", ";
                        if (clientsList == "[ ")
                            clientsList = "[empty]";
                        else
                        {
                            clientsList = clientsList.Substring(0, clientsList.Length - 2);
                            clientsList += " ]";
                        }
                        Message clientsListMessage = new Message(NetworkComms.NetworkIdentifier, "Server", "CHAT", clientsList, messageSendIndex++);
                        connection.SendObject("Message", clientsListMessage);
                    }), new object[] { currentUsers });
                }
            }
            /// <summary>
            /// Init a game with other players
            /// </summary>
            private void HandleInitHand(Connection connection, Message incomingMessage)
            {
                string actualPlayer = players[0];
                string itsHisTurn = "It's " + actualPlayer + "'s turn";
                BroadcastMessage(connection, incomingMessage, itsHisTurn, itsHisTurn);
                BroadcastMessage(connection, incomingMessage, actualPlayer, actualPlayer, "HAND");
            }

            /// <summary>
            /// Deal with user hand
            /// </summary>
            private bool HandleHand(Connection connection, Message incomingMessage)
            {
                actualPlayerIndex++;
                if (actualPlayerIndex >= playersArray.Length)
                {
                    actualPlayerIndex = 0;
                    return true;
                }
                string actualPlayer = players[actualPlayerIndex];
                string itsMonChipsTurn = "It's " + actualPlayer + "'s turn";
                BroadcastMessage(connection, incomingMessage, itsMonChipsTurn, itsMonChipsTurn);
                BroadcastMessage(connection, incomingMessage, actualPlayer, actualPlayer, "HAND");
                return false;
            }
            /// <summary>
            /// Begin a game with other users
            /// </summary>
            private void InitGame(Connection connection, Message incomingMessage)
            {
                gameReady = true;
                playersArray = players.ToArray();
                deck = new Deck(playersArray);
                cardsInRound = new Card[playersArray.Length];
                string everyoneIsReady = "Everyone is ready to play, game starting";
                BroadcastMessage(connection, incomingMessage, everyoneIsReady, everyoneIsReady);
                AppendLineToRichChatBox(everyoneIsReady);
                HandleInitHand(connection, incomingMessage);
            }
            /// <summary>
            /// Add user to a game
            /// </summary>
            private void AddToGame(Connection connection, Message incomingMessage)
            {
                if (gameReady == true) return;
                string msg = incomingMessage.SourceName + " is ready to play";
                AppendLineToRichChatBox(msg);
                BroadcastMessage(connection, incomingMessage, msg, "You are now ready to play");
                players.Add(incomingMessage.SourceName);
                int connectedClients = lastPeerMessageDict.Count - 1;
                if (players.Count == connectedClients && connectedClients >= 2)
                    InitGame(connection, incomingMessage);
            }
            /// <summary>
            /// Check if theres a winner in the game
            /// </summary>
            private string CheckRoundWinner(Connection connection, Message incomingMessage)
            {
                Card greatest = cardsInRound[0];
                for (int i = 0; i < cardsInRound.Length; i++)
                {
                    if (cardsInRound[i] != null)
                    {
                        if (cardsInRound[i].IsGreater(greatest))
                            greatest = cardsInRound[i];
                    }
                }
                string notifyWhosTheWinner = greatest.Owner + " won the round.";
                BroadcastMessage(connection, incomingMessage, notifyWhosTheWinner, notifyWhosTheWinner);
                foreach (Card card in cardsInRound)
                {
                    if (!greatest.Equals(card))
                        deck.GiveCard(card, greatest.Owner);
                }
                string winner;
                if ((winner = deck.IsThereAWinner()) != null) // TODO change comparison
                    return winner;
                else
                    AppendLineToRichChatBox("Theres no winner, game continues !");
                return null;
            }

            /// <summary>
            /// Finish a game
            /// </summary>
            private void EndGame(Connection connection, Message incomingMessage, string winner)
            {
                string notifyEnd = winner + " won the game !";
                BroadcastMessage(connection, incomingMessage, notifyEnd, notifyEnd, "END");
                if (aiGame == true)
                    aiGame = false;
            }
            /// <summary>
            /// Deal with players' card
            /// </summary>
            private void PlayACard(Connection connection, Message incomingMessage, bool aiTurn = false)
            {
                string notifyPlay = "";
                cardsInRound[actualPlayerIndex] = deck.pickRandomCard(playersArray[actualPlayerIndex]);
                if (aiTurn == false)
                    notifyPlay = incomingMessage.SourceName + " played a card : " + cardsInRound[actualPlayerIndex];
                else
                    notifyPlay = "AI played a card : " + cardsInRound[actualPlayerIndex];
                BroadcastMessage(connection, incomingMessage, notifyPlay, notifyPlay);
                bool isRoundOver = HandleHand(connection, incomingMessage);
                if (isRoundOver)
                {
                    string winner = CheckRoundWinner(connection, incomingMessage);
                    if (winner != null) // TODO change comparison 
                        EndGame(connection, incomingMessage, winner);
                    else
                        HandleInitHand(connection, incomingMessage);
                }
            }

            /// <summary>
            /// Send a player the number of its cards
            /// </summary>
            private void SendClientInventory(Connection connection, Message message)
            {
                int numberOfCards = 0;
                foreach (string user in playersArray)
                {
                    if (user == message.SourceName)
                        numberOfCards = deck.NumberOfCardsFor(user);
                }
                string inventory = "number of cards in deck : " + numberOfCards;
                BroadcastMessage(connection, message, "", inventory);
            }

            /// <summary>
            /// Init a game with AI
            /// </summary>
            private void AIGameInit(Connection connection, Message incomingMessage)
            {
                if (aiGame == true) return;
                string msg = incomingMessage.SourceName + " wants to fight against AI";
                AppendLineToRichChatBox(msg);
                BroadcastMessage(connection, incomingMessage, msg, "You are now playing with AI");
                aiGame = true;
                players.Add(incomingMessage.SourceName);
                players.Add("Bataille AI");
                playersArray = players.ToArray();
                deck = new Deck(playersArray);
                cardsInRound = new Card[playersArray.Length];
                HandleInitHand(connection, incomingMessage);
            }

            /// <summary>
            /// Deal with a user incoming message
            /// </summary>
            private void HandleIncomingMessage(PacketHeader header, Connection connection, Message incomingMessage)
            {
                lock (lastPeerMessageDict)
                {
                    switch (incomingMessage.coincheHeader)
                    {
                        case "CHAT":
                            AppendToChat(connection, incomingMessage);
                            RefreshMessagesFromBox();
                            break;
                        case "CONNECT":
                            AppendNewClient(connection, incomingMessage);
                            RefreshMessagesFromBox();
                            break;
                        case "LIST":
                            SendClientsList(connection, incomingMessage);
                            break;
                        case "PLAYER_PLAY":
                            AddToGame(connection, incomingMessage);
                            break;
                        case "AI_PLAY":
                            AIGameInit(connection, incomingMessage);
                            break;
                        case "PUTTED":
                            PlayACard(connection, incomingMessage);
                            if (aiGame == true)
                                PlayACard(connection, incomingMessage, true);
                            break;
                        case "INVENTORY":
                            SendClientInventory(connection, incomingMessage);
                            break;
                        default:
                            break;
                    }
                }
            }
            /// <summary>
            /// Callback when a connection is closed
            /// </summary>
            private void HandleConnectionClosed(Connection connection)
            {
                lock (lastPeerMessageDict)
                {
                    ShortGuid remoteIdentifier = connection.ConnectionInfo.NetworkIdentifier;
                    if (lastPeerMessageDict.ContainsKey(remoteIdentifier))
                    {
                        AppendLineToRichChatBox("Connection with '" + lastPeerMessageDict[remoteIdentifier].SourceName + "' has been closed.");
                        if (lastPeerMessageDict[remoteIdentifier].SourceName == playersArray[0] && aiGame == true)
                            aiGame = false;
                    }
                    else
                        AppendLineToRichChatBox("Connection with '" + connection.ToString() + "' has been closed.");

                    lastPeerMessageDict.Remove(connection.ConnectionInfo.NetworkIdentifier);
                }
                RefreshMessagesFromBox();
            }
            /// <summary>
            /// Broadcast a message to all/specific client(s)
            /// </summary>
            /// <param name="messageForAll">message for all clients</param>
            /// <param name="messageForSender">message for specific client</param>
            private void BroadcastMessage(Connection connection, Message incomingMessage, string messageForAll, string messageForSender, string header = "CHAT")
            {
                var allRelayConnections = (from current in NetworkComms.GetExistingConnection() select current).ToArray();
                incomingMessage.IncrementRelayCount();
                foreach (var relayConnection in allRelayConnections)
                {
                    try
                    {
                        string msg;
                        if (connection == relayConnection)
                            msg = messageForSender;
                        else
                            msg = messageForAll;
                        if (!string.IsNullOrEmpty(msg))
                        {
                            Message message = new Message(NetworkComms.NetworkIdentifier, "Server", header, msg, messageSendIndex++);
                            lastPeerMessageDict[NetworkComms.NetworkIdentifier] = message;
                            relayConnection.SendObject("Message", message);
                        }
                    }
                    catch (CommsException)
                    {
                        // Catch the comms exception, ignore and continue 
                    }
                }
            }
            /// <summary>
            /// Send a message to clients
            /// </summary>
            private void SendMessage()
            {
                if (messageText.Text.Trim() == "")
                    return;
                Message messageToSend = new Message(NetworkComms.NetworkIdentifier, localName.Text, "", messageText.Text, messageSendIndex++);
                lock (lastPeerMessageDict) lastPeerMessageDict[NetworkComms.NetworkIdentifier] = messageToSend;
                AppendLineToRichChatBox("[" + (DateTime.Now).ToString(hourFormat) + "] " + messageToSend.SourceName + " : " + messageToSend.MessageContent);
                RefreshMessagesFromBox();
                this.messageText.Text = "";
                var otherConnectionInfos = (from current in NetworkComms.AllConnectionInfo() select current).ToArray();
                foreach (ConnectionInfo info in otherConnectionInfos)
                {
                    try { TCPConnection.GetConnection(info).SendObject("Message", messageToSend); }
                    catch (CommsException) { MessageBox.Show("A CommsException occurred while trying to send message to " + info, "CommsException", MessageBoxButton.OK); }
                }
            }
        }
    }
}