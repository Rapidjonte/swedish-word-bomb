namespace ConsoleApp19
{
    using System.Text.RegularExpressions;

    internal class Program
    {
        public static bool guessed = false;

        public async static Task check(string word, string ordbok)
        {
            // Skapa en HttpClient-instans
            using (HttpClient client = new HttpClient())
            {
                Console.WriteLine("input word: " + word);

                word = Regex.Replace(word, @"[^a-zA-ZåäöÅÄÖéÉ' \-]", "");
                word = word.Trim();
                word = word.ToLower();
                word = word.Replace("'", "%27");
                word = word.Replace(" ", "+");

                var url = $@"https://svenska.se/tri/f_{ordbok.ToLower()}.php?sok={word}&pz=1";

                Console.WriteLine("parsed word: " + word + "\nurl: " + url + "\n");

                try
                {
                    // Skicka GET-request till den specifika URL:en
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Kontrollera om anropet var framgångsrikt
                    if (response.IsSuccessStatusCode)
                    {
                        // Läs svaret som en sträng
                        string responseContent = await response.Content.ReadAsStringAsync();
                        //Console.WriteLine("Svar från servern: " + responseContent);
                        if (responseContent.Contains($@"</strong> i {ordbok.ToUpper()} gav inga svar.<"))
                        {
                            Console.WriteLine(ordbok.ToUpper() + " - Fel!\n");
                            guessed = false;
                        }
                        else
                        {
                            Console.WriteLine(ordbok.ToUpper() + " - KORREKT!\n");
                            guessed = true;
                        }
                        // </strong> i SAOL gav inga svar.<br /><br>
                    }
                    else
                    {
                        Console.WriteLine("Fel vid GET-request: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ett fel uppstod: " + ex.Message);
                }
            }
        }

        static async Task Main(string[] args)
        {
            while (true)
            {
                string input = Console.ReadLine();
                await check(input, "saol");
                await check(input, "so");
                await check(input, "saob");
            }
        }
    }
}
