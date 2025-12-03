using TwentyOne.Models;
using TwentyOne.Models.Enums;

public class ShoeTest
{
    [Fact]
    public void ShoeInitialized()
    {
        int rankCount = Enum.GetValues<Rank>().Length;
        int suitCount = Enum.GetValues<Suit>().Length;
        
        // Test default constructor (1 deck)
        int totalCardCount = rankCount * suitCount;

        Shoe shoe = new();

        Assert.Equal(1, shoe.DeckCount);
        Assert.Equal(totalCardCount, shoe.TotalCardCount);
        Assert.Equal(totalCardCount, shoe.UndealtCardCount);

        // Test constructor with specified deck count (1 deck)
        int shoeDeckCount = 1;
        totalCardCount = rankCount * suitCount * shoeDeckCount;

        shoe = new(shoeDeckCount);

        Assert.Equal(shoeDeckCount, shoe.DeckCount);
        Assert.Equal(totalCardCount, shoe.TotalCardCount);
        Assert.Equal(totalCardCount, shoe.UndealtCardCount);


        // Test constructor with multiple deck count (3 decks)
        shoeDeckCount = 3;
        totalCardCount = rankCount * suitCount * shoeDeckCount;

        shoe = new(shoeDeckCount);

        Assert.Equal(shoeDeckCount, shoe.DeckCount);
        Assert.Equal(totalCardCount, shoe.TotalCardCount);
        Assert.Equal(totalCardCount, shoe.UndealtCardCount);
    }
}