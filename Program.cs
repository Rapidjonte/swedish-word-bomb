﻿namespace ConsoleApp19
{
    using System.Text.RegularExpressions;
    using System.Xml;
    using System;
    using System.Linq;
    using System.Text;

    internal class Program
    {
        public static bool guessed = false;
        public static int rätt = 0;
        public static int fel = 0;
        public static List<string> words = new List<string>();
        public static string characters;

        public async static Task check(string word, string ordbok)
        {
            // Skapa en HttpClient-instans
            using (HttpClient client = new HttpClient())
            {
                // Console.WriteLine("input word: " + word);

                if (ordbok != "slangopedia")
                {
                    word = Regex.Replace(word, @"[^a-zA-ZåäöÅÄÖéÉ' \-]", "");
                    characters = Regex.Replace(characters, @"[^a-zA-ZåäöÅÄÖéÉ' \-]", "");
                }

                word = word.Trim();
                word = word.ToLower();
                characters = characters.Trim(); 
                characters = characters.ToLower();

                if (words.Contains(word) || !word.Contains(characters))
                {
                    return;
                } 
                else
                {
                    words.Add(word);
                }

                string url;
                if (ordbok == "slangopedia")
                {
                    url = $@"https://www.slangopedia.se/ordlista/?ord={EncodeToCustomUrl(word)}";
                    //Console.WriteLine(url);
                } else
                {
                    word = word.Replace("'", "%27");
                    word = word.Replace(" ", "+");

                    url = $@"https://svenska.se/tri/f_{ordbok.ToLower()}.php?sok={word}";
                }

                // Console.WriteLine("parsed word: " + word + "\nurl: " + url + "\n");

                try
                {
                    // Skicka GET-request till den specifika URL:en
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Kontrollera om anropet var framgångsrikt
                    if (response.IsSuccessStatusCode)
                    {
                        // Läs svaret som en sträng
                        string responseContent = await response.Content.ReadAsStringAsync();
                        //Console.WriteLine(responseContent);

                        if (ordbok != "slangopedia")
                        {
                            if (responseContent.Contains($@"</strong> i {ordbok.ToUpper()} gav inga svar.<"))
                            {
                                // Console.WriteLine(ordbok.ToUpper() + " - Fel!\n");
                                fel++;
                            }
                            else
                            {
                                // Console.WriteLine(ordbok.ToUpper() + " - KORREKT!\n");
                                rätt++;
                                guessed = true;
                            }
                            // </strong> i SAOL gav inga svar.<br /><br>
                        } 
                        else 
                        {
                            if (responseContent.Contains($@"</i> finns inte i ordlistan. "))
                            {
                                fel++;
                                //Console.WriteLine("finns inte");
                            }
                            else
                            {
                                rätt++;
                                //Console.WriteLine("finns");
                                guessed = true;
                            }
                        }
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

        public static string GetRandomWrittenForm(string xmlFilePath)
        {
            try
            {
                // Ladda XML-dokumentet
                string xmlContent = System.IO.File.ReadAllText(xmlFilePath);

                // Ladda XML i en XmlDocument
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);

                // Hitta alla <feat> med attributet "att" = "writtenForm"
                XmlNodeList writtenFormNodes = xmlDoc.SelectNodes("//feat[@att='writtenForm']");

                // Lista för att lagra alla "writtenForm"-värden
                List<string> writtenForms = new List<string>();

                // Lägg till alla "writtenForm"-värden till listan
                foreach (XmlNode node in writtenFormNodes)
                {
                    writtenForms.Add(node.Attributes["val"].Value);
                }

                // Kontrollera om vi har några ord
                if (writtenForms.Count > 0)
                {
                    // Skapa en random generator
                    Random random = new Random();

                    // Välj ett slumpmässigt ord från listan
                    return writtenForms[random.Next(writtenForms.Count)];
                }
                else
                {
                    Console.WriteLine("Ingen 'writtenForm' hittades.");
                    return null;  // Ingen "writtenForm" hittades
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ett fel uppstod: " + ex.Message);
                return null;
            }
        }

        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                string randomWord = GetRandomWrittenForm("ao.xml");
                // Console.WriteLine(randomWord);
                Random rng = new Random();
                characters = randomWord.Substring(rng.Next(0, randomWord.Length-1));
                if (characters.Length > 3)
                {
                    characters = characters.Remove(rng.Next(2, 4));
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(characters + "\n");
                Console.ForegroundColor = ConsoleColor.White;

                guessed = false;
                while (!guessed)
                {
                    string input = Console.ReadLine();
                    if (input == "/?" || input == "/ff" || input == "/die" || input == "/kill" || input == "/forfeit" || input == "/resign" || input == "/giveup" || input == "/suicide" || input == "/skip" || input == "/pass" || input == "/idk")
                    {
                        Console.WriteLine(randomWord + "\n");
                        guessed = true;
                        Console.ReadKey(true);
                    }

                    if (!guessed) await check(input, "saol");
                    if (!guessed) await check(input, "so");
                    if (!guessed) await check(input, "slangopedia");
                    if (!guessed) await check(input, "saob");
                }
            }
        }
        static string EncodeToCustomUrl(string input)
        {
            // Skapa en StringBuilder för att bygga den kodade strängen
            StringBuilder encoded = new StringBuilder();

            foreach (char c in input)
            {
                // Kontrollera om tecknet är alfanumeriskt (bokstäver och siffror)
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                {
                    encoded.Append(c); // Behåll alfanumeriska tecken (både bokstäver och siffror)
                }
                else if (c == ' ')  // Hantera mellanslag genom att byta ut det mot "+"
                {
                    encoded.Append('+');
                }
                else if (c == '&')  // Kodar tecknet '&' som '%26'
                {
                    encoded.Append("%26");
                }
                else if (c == '.')  // Behåll punkten utan att koda den
                {
                    encoded.Append(c);
                }
                else if (c == '-')  // Behåll bindestreck utan att koda det
                {
                    encoded.Append(c);
                }
                else
                {
                    // För alla andra tecken, använd procentkodning
                    encoded.AppendFormat("%{0:X2}", (int)c);
                }
            }

            return encoded.ToString();
        }
    }
}