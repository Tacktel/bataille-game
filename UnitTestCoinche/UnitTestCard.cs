using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Coinche;

namespace Coinche
{
    [TestClass]
    public class UnitTestCard
    {
        [TestMethod]
        public void TestCardCreation()
        {
            Assert.IsNotNull(new Card(CardSuit.Clubs, CardRank.Ace, "Pat Hat"));
            /* C# constructers return void and such a factory is handy to control creation */
            Card unOwnedCard = new Card(CardSuit.Diamonds, CardRank.Jack, "");
            Assert.IsNotNull(unOwnedCard);
            Assert.IsTrue(unOwnedCard.Owner == "UNKNOW");
            Assert.IsNotNull(new Card(CardSuit.Clubs, (CardRank)30, "Pat Hat"));
        }
    }
}
