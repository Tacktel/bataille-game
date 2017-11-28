using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Tools;
using ProtoBuf;

namespace Coinche
{

    [ProtoContract]
    public class Message
    {
        public Message() { }

        /// <summary>
        /// Message constructer
        /// </summary>
        /// <param name="header">message type</param>
        /// <param name="message">message readable contents</param>
        public Message(ShortGuid sourceIdentifier, string sourceName, string header, string message, long messageIndex)
        {
            this._sourceIdentifier = sourceIdentifier;
            this.SourceName = sourceName;
            this.coincheHeader = header;
            this.MessageContent = message;
            this.MessageIndex = messageIndex;
            this.RelayCount = 0;
        }

        public Message(string sourceName, string header, string message, long messageIndex)
        {
            this._sourceIdentifier = "";
            this.SourceName = sourceName;
            this.coincheHeader = header;
            this.MessageContent = message;
            this.MessageIndex = messageIndex;
            this.RelayCount = 0;
        }

        [ProtoMember(1)]
        public string coincheHeader;

        [ProtoMember(2)]
        string _sourceIdentifier;

        public ShortGuid SourceIdentifier { get { return new ShortGuid(_sourceIdentifier); } }

        [ProtoMember(3)]
        public string SourceName { get; private set; }

        [ProtoMember(4)]
        public string MessageContent { get; private set; }

        [ProtoMember(5)]
        public long MessageIndex { get; private set; }

        [ProtoMember(6)]
        public int RelayCount { get; private set; }

        public void IncrementRelayCount()
        {
            RelayCount++;
        }
    }

}