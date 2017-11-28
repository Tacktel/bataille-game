using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coinche
{
    public enum CardSuit
    {
        Spades,
        Hearts,
        Diamonds,
        Clubs
    }
    public enum CardRank
    {
        Ace = 14,
        /*Two = 2, Three = 3, Four = 4, Five = 5, Six = 6,*/
        Seven = 7, Eight = 8, Nine = 9, Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13
    }
    public class Card : IEquatable<Card>
    {
        /// <summary>
        /// Card constructor
        /// </summary>
        public Card(CardSuit suit, CardRank rank, string owner)
        {
            if (string.IsNullOrEmpty(owner))
            {
                Console.WriteLine("Card has been constructed with an UNKNOW owner");
                owner = "UNKNOW";
            }
            Suit = suit;
            Rank = rank;
            Owner = owner;
        }

        public CardSuit Suit { get; private set; }
        public CardRank Rank { get; private set; }
        public string Owner { get; set; }
        public const int numberOfCards = 32;
        /// <summary>
        /// Check for cards equality
        /// </summary>
        public static bool operator ==(Card left, Card right)
        {
            if (((object)left == null) || ((object)right == null))
            {
                return false;
            }
            if (ReferenceEquals(left, right))
            {
                return true;
            }
            return left.Equals(right);
        }
        /// <summary>
        /// Check for cards inequality
        /// </summary>
        public static bool operator !=(Card left, Card right)
        {
            return !(left == right);
        }
        /// <summary>
        /// Check for card equality using IEq
        /// </summary>
        public bool Equals(Card other)
        {
            if (other == null)
                return false;
            return Rank == other.Rank && Suit == other.Suit;
        }
        /// <summary>
        /// Compare cards
        /// </summary>
        /// <param name="other">card to compare</param>
        public bool IsLower(Card other)
        {
            if (Rank < other.Rank)
                return true;
            return false;
        }
        /// <summary>
        /// Compare cards
        /// </summary>
        /// <param name="other">card to compare</param>
        public bool IsGreater(Card other)
        {
            return !IsLower(other);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Card);
        }
        /// <summary>
        /// hashcode the card
        /// </summary>
        public override int GetHashCode()
        {
            return Suit.GetHashCode() ^ Rank.GetHashCode() ^ Owner.GetHashCode();
        }
        /// <summary>
        /// Allow a card to be printable
        /// </summary>
        public override string ToString()
        {
            return Rank.ToString() + " of " + Suit.ToString();
        }
    }
}
