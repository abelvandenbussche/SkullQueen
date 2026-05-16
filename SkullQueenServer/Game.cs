using Shared;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;
using System.Xml.Linq;

namespace SkullQueenServer
{
    class CatImageResult
    {
        public string id { get; set; }
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }

        public CatImageResult(string id, string url, int width, int height)
        {
            this.id = id;
            this.url = url;
            this.width = width;
            this.height = height;
        }
    }
    public class Game
    {
        private List<Player> players;
        private Round currentRound;


        public Game(List<Player> players)
        {
            this.players = players;
            Utility.BroadCast(players, Command.StartGame);

            // Send all players their opponents
            foreach (Player player in players)
            {
                Utility.BroadCast(players, Command.DisplayOpponent, player.name, player);
            }
            // Getting the profile pics for the players, this is done asynchronously so the game can start while the images are being fetched
            Task.Run(() => SendProfilePics());
        }
        public async Task SendProfilePics()
        {
            List<string> urls = await GetCats(players.Count);
            for (int i = 0; i < players.Count; i++)
            {
                Player player = players[i];
                string url = urls[i];
                Utility.BroadCast(players, Command.SendProfilePicture, player.name + " " + url);
            }
        }
        public async Task<List<string>> GetCats(int amount)
        {
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync($"https://api.thecatapi.com/v1/images/search?mime_types=jpg&format=json&order=RANDOM&page=0&limit={amount}&width=500&height=500");
                string data = await response.Content.ReadAsStringAsync();
                List<CatImageResult> images = JsonSerializer.Deserialize<List<CatImageResult>>(data)!;
                List<string> imageUrls = new();
                foreach(CatImageResult image in images)
                {
                    imageUrls.Add(image.url);
                }
                return imageUrls;
            }
            catch
            {
                // Standard image
                List<string> images = new();
                for (int i = 0; i < amount; i++)
                {
                    images.Append("https://http.cat/images/102.jpg");
                }
                return images;
            }
        }
        public async Task StartGame()
        {
            for (int i = 0; i < players.Count; i++)
            {
              this.currentRound = new Round(players);
              await currentRound.StartRound();

              //Rotating the players so the start players is different
              Player first = players[0];
              players.RemoveAt(0);
              players.Add(first);
            }
            // Sending the scores of the players for the end of the game
            string message = "";
            foreach (Player player in players)
            {
              message += player.name + " ";
              message += player.score + " ";
            }
            Utility.BroadCast(players, Command.EndScoring, message);
        }
    }
}
