using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Coinche;

namespace Coinche
{
    [TestClass]
    public class UnitTestDeck
    {
        [TestMethod]
        public void TestDeckCreation()
        {
            string[] users = new string[] { "jésus", "jean", "luc" };
            Deck d = new Deck(users);
            Assert.IsNotNull(d.Cards);
            Assert.IsNotNull(d.Players);
            Assert.IsNotNull(d.CardsPerPlayer);
        }
        [TestMethod]
        public void TestEmptyDeckCreation()
        {
            string[] users = new string[] { };
            Deck d = new Deck(users);
            Assert.IsNull(d.Cards);
            Assert.IsNull(d.Players);
            Assert.IsNull(d.CardsPerPlayer);
        }

        [TestMethod]
        public void TestGlobalCardsNumber()
        {
            string[] users = new string[] { "jésus", "simon", "marc" };
            Deck d = new Deck(users);
            Assert.IsTrue(d.Cards.Count == d.Players.Length * Card.numberOfCards);
        }

        [TestMethod]
        public void TestPlayerCardsNumber()
        {
            string[] users = new string[] { "jésus", "jean" };
            Deck d = new Deck(users);
            Assert.IsTrue(d.NumberOfCardsFor("jésus") == 32);
            Assert.IsTrue(d.NumberOfCardsFor("jésus") == d.Cards.Count / d.Players.Length);
            Assert.IsNotNull(d.getCardsOf("jésus"));
            Assert.IsTrue(d.getCardsOf("jésus").Length == d.Cards.Count / d.Players.Length);
        }

        [TestMethod]
        public void TestErronousPlayerCardsNumber()
        {
            string[] users = new string[] { "jésus", "jean" };
            Deck d = new Deck(users);
            Assert.IsTrue(d.NumberOfCardsFor("mathieu") == -1);
            Assert.IsTrue(d.NumberOfCardsFor("mathieu") != d.Cards.Count / d.Players.Length);
            Assert.IsNull(d.getCardsOf("mathieu"));
        }

        [TestMethod]
        public void TestTransferCard()
        {
            string[] users = new string[] { "jésus", "judas" };
            Deck d = new Deck(users);
            int JesusCards = d.NumberOfCardsFor("jésus");
            int JudasCards = d.NumberOfCardsFor("judas");
            Assert.AreEqual(d.NumberOfCardsFor("jésus"), d.NumberOfCardsFor("judas"));
            Card takenCardFromJudas = d.pickRandomCard("judas");
            d.GiveCard(takenCardFromJudas, "jésus");
            Assert.AreNotEqual(d.NumberOfCardsFor("jésus"), d.NumberOfCardsFor("judas"));
            Assert.IsTrue(d.NumberOfCardsFor("jésus") == d.NumberOfCardsFor("judas") + 2);
            Assert.IsTrue(d.NumberOfCardsFor("jésus") == JesusCards + 1);
            Assert.IsTrue(d.NumberOfCardsFor("judas") == JudasCards - 1);
        }

        [TestMethod]
        public void TestPickCard()
        {
            string[] users = new string[] { "lucifer", "boudhha", "belial", "behemoth", "asmodeus" };
            Deck d = new Deck(users);
            Assert.IsNull(d.pickRandomCard("satanas"));
            Assert.IsNotNull(d.pickRandomCard("lucifer"));
        }

        [TestMethod]
        public void TestWinningTheGame()
        {
            string[] users = new string[] { "lucifer", "boudhha", "belial", "behemoth", "asmodeus" };
            Deck d = new Deck(users);
            Assert.IsTrue(d.IsThereAWinner() == null);
            d.CardsPerPlayer[3] = 0;
            Assert.IsTrue(d.IsThereAWinner() == "behemoth");
        }
    }
}