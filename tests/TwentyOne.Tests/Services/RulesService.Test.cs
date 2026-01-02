using TwentyOne.Constants;
using TwentyOne.Models;
using TwentyOne.Services;

namespace TwentyOne.Tests.Services;

public class RulesServiceTest
{
    [Theory]
    // General card value scenarios
    [InlineData(new Rank[] { Rank.Two, Rank.Three }, 5)]
    [InlineData(new Rank[] { Rank.Two, Rank.Three, Rank.Four }, 9)]
    [InlineData(new Rank[] { Rank.Two, Rank.Three, Rank.Four, Rank.Five }, 14)]
    [InlineData(new Rank[] { Rank.Five, Rank.Six, Rank.Ten }, 21)]
    // Ace scenarios
    [InlineData(new Rank[] { Rank.Ace, Rank.Five }, 16)]
    [InlineData(new Rank[] { Rank.Ace, Rank.Ten }, 21)]
    [InlineData(new Rank[] { Rank.Ace, Rank.Jack }, 21)]
    [InlineData(new Rank[] { Rank.Ace, Rank.Queen }, 21)]
    [InlineData(new Rank[] { Rank.Ace, Rank.King }, 21)]
    // Mutiple Ace scenarios
    [InlineData(new Rank[] { Rank.Ace, Rank.Ace, Rank.Nine }, 21)]
    [InlineData(new Rank[] { Rank.Ace, Rank.Ace, Rank.Ace }, 13)]
    // 7 Ace scenario (unlikely but possible in testing)
    [InlineData(new Rank[] { 
        Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace,
        Rank.Ace, Rank.Ace }, 17)]
    // 11 Ace scenario (unlikely but possible in testing)
    [InlineData(new Rank[] { 
        Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, 
        Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace,
        Rank.Ace }, 21)]
    // 12 Ace scenario (unlikely but possible in testing)
    [InlineData(new Rank[] { 
        Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, 
        Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace,
        Rank.Ace, Rank.Ace }, 12)]
    public void HandValue_CalculatesCorrectly(Rank[] ranks, int expectedValue)
    {
        Hand hand = new();
        foreach (var rank in ranks)
        {
            hand.AddCard(new Card(rank, Suit.Hearts));
        }

        int value = RulesService.HandValue(hand);

        Assert.Equal(expectedValue, value);
    }

    [Theory]
    [InlineData(new Rank[] { Rank.Ten, Rank.King, Rank.Two }, true)]
    [InlineData(new Rank[] { Rank.Ten, Rank.King }, false)]
    // (21) 11 Ace scenario (unlikely but possible in testing)
    [InlineData(new Rank[] { 
        Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, 
        Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace,
        Rank.Ace }, false)]
    // (12) 12 Ace scenario (unlikely but possible in testing)
    [InlineData(new Rank[] { 
        Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, 
        Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace,
        Rank.Ace, Rank.Ace }, false)]
    public void HandIsBust_WorksCorrectly(Rank[] ranks, bool expectedIsBust)
    {
        Hand hand = new();
        foreach (var rank in ranks)
        {
            hand.AddCard(new Card(rank, Suit.Hearts));
        }

        bool isBust = RulesService.HandIsBust(hand);

        Assert.Equal(expectedIsBust, isBust);
    }

    [Theory]
    [InlineData(new Rank[] { Rank.Ten, Rank.King, Rank.Ace }, true)]
    [InlineData(new Rank[] { Rank.Ten, Rank.King }, false)]
    // 11 Ace scenario (unlikely but possible in testing)
    [InlineData(new Rank[] { 
        Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, 
        Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace,
        Rank.Ace }, true)]
    public void HandIsTwentyOne_WorksCorrectly(Rank[] ranks, bool expectedIsTwentyOne)
    {
        Hand hand = new();
        foreach (var rank in ranks)
        {
            hand.AddCard(new Card(rank, Suit.Hearts));
        }

        bool isTwentyOne = RulesService.HandIsTwentyOne(hand);

        Assert.Equal(expectedIsTwentyOne, isTwentyOne);
    }

    [Theory]
    [InlineData(new Rank[] { Rank.Ace, Rank.King }, true)]  // Natural 21
    [InlineData(new Rank[] { Rank.Ten, Rank.King }, false)] // Not 21
    [InlineData(new Rank[] { Rank.Ace, Rank.Nine, Rank.Ace }, false)] // 21 but not natural
    public void HandIsNatural_WorksCorrectly(Rank[] ranks, bool expectedIsNatural)
    {
        Hand hand = new();
        foreach (var rank in ranks)
        {
            hand.AddCard(new Card(rank, Suit.Hearts));
        }

        Assert.Equal(expectedIsNatural, RulesService.HandIsNatural(hand));
    }

    [Theory]
    // 2 cards Value is 9, 10, or 11
    [InlineData(new Rank[] { Rank.Three, Rank.Six }, true)]
    [InlineData(new Rank[] { Rank.Five, Rank.Six }, true)]
    // Value not 9, 10, or 11
    [InlineData(new Rank[] { Rank.Ten, Rank.King }, false)]
    // Hand is not 2 cards
    [InlineData(new Rank[] { Rank.Ace, Rank.Nine, Rank.Ace }, false)]
    // TODO: Research to make sure the test below should even be considered
    // [InlineData(new Rank[] { Rank.Ace, Rank.King }, true)]
    public void CanDoubleDown_WorksCorrectly(Rank[] ranks, bool expectedCanDoubleDown)
    {
        Hand hand = new();
        foreach (var rank in ranks)
        {
            hand.AddCard(new Card(rank, Suit.Hearts));
        }

        Assert.Equal(expectedCanDoubleDown, RulesService.CanDoubleDown(hand));
        
    }

    [Theory]
    [InlineData(new Rank[] { Rank.Ace, Rank.Ace }, 1, true)]
    [InlineData(new Rank[] { Rank.Ten, Rank.Ten }, 3, true)]
    [InlineData(new Rank[] { Rank.Ten, Rank.Ten }, 4, false)]
    [InlineData(new Rank[] { Rank.Ten, Rank.Ten }, 5, false)]
    public void CanSplitHand_WorksCorrectly(Rank[] ranks, int handCount, bool expectedCanSplitValue)
    {
        Hand hand = new();
        foreach (var rank in ranks)
        {
            hand.AddCard(new Card(rank, Suit.Hearts));
        }

        Assert.Equal(expectedCanSplitValue, RulesService.CanSplitHand(hand, handCount));
    }


    [Theory]
    [InlineData(new Rank[] { Rank.Two, Rank.Three }, true)] // 5 < 17
    [InlineData(new Rank[] { Rank.Ten, Rank.Seven }, false)] // 17 == 17
    [InlineData(new Rank[] { Rank.King, Rank.Nine }, false)] // 19 > 17
    [InlineData(new Rank[] { Rank.Ace, Rank.Nine, Rank.Ace }, false)] // 21 > 17 Three cards
    // TODO: Research to make sure the tests below should even be considered
    // (16) 6 Ace scenario (unlikely but possible in testing)
    // [InlineData(new Rank[] { 
    //     Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, 
    //     Rank.Ace, Rank.Ace }, true)]
    // (8) 7 Ace scenario (unlikely but possible in testing)
    // [InlineData(new Rank[] { 
    //     Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Ace, 
    //     Rank.Ace, Rank.Ace, Rank.Ace }, true)]
    public void DealerShouldDraw_WorksCorrectly(Rank[] ranks, bool expectedShouldDraw)
    {
        Hand dealerHand = new();
        foreach (var rank in ranks)
        {
            dealerHand.AddCard(new Card(rank, Suit.Hearts));
        }

        Assert.Equal(expectedShouldDraw, RulesService.DealerShouldDraw(dealerHand));
    }
}