using System;
using System.Collections.Generic;
using System.Linq;

namespace Coinche
{
    public class Deck
    {
        public List<Card> Cards { get; set; }
        public string[] Players { get; set; }
        public int[] CardsPerPlayer { get; set; }
        public int NumberOfPlayers { get; set; }
        /// <summary>
        /// Deck constructor
        /// </summary>
        /// <param name="players">players to use the deck</param>
        public Deck(string[] players)
        {
            if (players.Length == 0) return;
            Players = players;
            Cards = new List<Card>();
            NumberOfPlayers = players.Length;
            CardsPerPlayer = new int[NumberOfPlayers];
            Random randomizer = new Random();

            Console.WriteLine("number of cards in each deck : " + Card.numberOfCards);
            Console.WriteLine("Generating " + Card.numberOfCards * NumberOfPlayers + " cards.");

            for (int i = 0; i < NumberOfPlayers; ++i)
            {
                foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
                {
                    foreach (CardRank rank in Enum.GetValues(typeof(CardRank)))
                    {
                        int random = randomizer.Next(0, NumberOfPlayers);
                        while (CardsPerPlayer[random] >= 32)
                        {
                            random = randomizer.Next(0, NumberOfPlayers);
                        }
                        CardsPerPlayer[random]++;
                        Card c = new Card(suit, rank, players[random]);
                        Console.WriteLine(c.ToString());
                        Cards.Add(c);
                    }
                }
            }
        }
        /// <summary>
        /// Select random card from user's deck
        /// </summary>
        /// <param name="owner">deck owner where to pick the card</param>
        public Card pickRandomCard(string owner)
        {
            if (!Players.Contains(owner))
            {
                Console.WriteLine("No such user " + owner + " to pick a card");
                return null;
            }
            Random randomizer = new Random();
            int random = randomizer.Next(0, Cards.Count);
            while (Cards[random].Owner != owner)
            {
                random = randomizer.Next(0, Cards.Count);
            }
            return Cards[random];
        }
        /// <summary>
        /// Check for a winner
        /// </summary>
        public string IsThereAWinner()
        {
            for (int i = 0; i < NumberOfPlayers; ++i)
            {
                if (CardsPerPlayer[i] <= 0)
                    return Players[i];
            }
            return null;
        }
        /// <summary>
        /// Retrieve number of cards in player's deck
        /// </summary>
        /// <param name="player">player to fetch the number of its cards</param>
        public int NumberOfCardsFor(string player)
        {
            for (int i = 0; i < NumberOfPlayers; ++i)
            {
                if (Players[i] == player)
                    return CardsPerPlayer[i];
            }
            return -1;
        }
        /// <summary>
        /// Transfer ownership from a player to another one
        /// </summary>
        /// <param name="card">card to transfer</param>
        /// <param name="to">player to give the card</param>
        public void GiveCard(Card card, string to)
        {
            Console.WriteLine("Giving " + card.Owner + "'s card to " + to);
            addCard(to);
            removeCard(card.Owner);
            card.Owner = to;
        }
        /// <summary>
        /// Retrieve deck of specific player
        /// </summary>
        public Card[] getCardsOf(string player)
        {
            if (!Players.Contains(player))
            {
                Console.WriteLine("Cannot getCardsOf " + player + ". No such player");
                return null;
            }
            List<Card> playerCards = new List<Card>(); ;
            foreach (Card card in Cards)
            {
                if (card.Owner == player)
                {
                    playerCards.Add(card);
                }
            }
            return playerCards.ToArray();
        }
        /// <summary>
        /// Add card to player's sdeck
        /// </summary>
        /// <param name="to">player to give a card</param>
        private void addCard(string to)
        {
            for (int i = 0; i < NumberOfPlayers; ++i)
            {
                if (Players[i] == to)
                    CardsPerPlayer[i]++;
            }
        }
        /// <summary>
        /// Remove a card from player
        /// </summary>
        /// <param name="to">player to remove a card</param>
        private void removeCard(string to)
        {
            for (int i = 0; i < NumberOfPlayers; ++i)
            {
                if (Players[i] == to)
                    CardsPerPlayer[i]--;
            }
        }
    }
}
