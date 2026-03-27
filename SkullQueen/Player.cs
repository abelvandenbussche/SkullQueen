namespace SkullQueen
{
    abstract class Player
    {
        private Plank plank;
        private List<Card> hand;
        private int score;
    }
    class Host : Player
    {

    }
    class Client : Player
    {

    }
}