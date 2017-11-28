using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coinche
{
    class Program
    {
        /// <summary>
        /// Testing main that is separated from unit tests (see project UnitTestCoinche)
        /// </summary>
        static void Main(string[] args)
        {
            string[] users = new string[] { "jésus", "jean", "luc" };
            Deck d = new Deck(users);
            Console.WriteLine("Ok les amis");
            Card takenCardFromJean = d.pickRandomCard("jean");
            d.GiveCard(takenCardFromJean, "jésus");
            string p = d.IsThereAWinner();
            if (p != null)
                Console.WriteLine("The winner is " + p);
            else
                Console.WriteLine("Theres no winner");
            Console.WriteLine("Jean has " + d.NumberOfCardsFor("jean") + " cards in his hand");
            Console.WriteLine("Jésus has " + d.NumberOfCardsFor("jésus") + " cards in his hand");
            Console.WriteLine("Jésus' deck :");
            Card[] jesusCards = d.getCardsOf("jésus");
            foreach (var card in jesusCards)
            {
                Console.WriteLine(card);
            }
            Console.ReadKey();
        }
    }
}