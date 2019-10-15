using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dapper;
using FlashCards.Data;
using Microsoft.AspNetCore.Mvc;
using FlashCards.Models;
using Newtonsoft.Json;

namespace FlashCards.Controllers
{
    public class HomeController : Controller
    {
        private readonly SqlConnectionFactory _sqlConnectionFactory;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(SqlConnectionFactory sqlConnectionFactory, IHttpClientFactory httpClientFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            //await InsertWordsToDb();
            return View();
        }

        [HttpGet, Route("/get-random-word")]
        public async Task<IActionResult> GetRandomWord()
        {
            using (var con = _sqlConnectionFactory.GetConnection())
            {
                var q = "SELECT word FROM words ORDER BY random() LIMIT 1";
                return Ok(await con.QueryFirstOrDefaultAsync<WordDto>(q));
            }
        }

        public async Task<IActionResult> GetWordExamples(string word)
        {
            var client = _httpClientFactory.CreateClient("oxford");
            client.DefaultRequestHeaders.Add("app_id", "b5f71d5e");
            client.DefaultRequestHeaders.Add("app_key", "ebb0797e40dd8addbf8b03e7633399e9");
            var response = await client.GetStringAsync("https://od-api.oxforddictionaries.com/api/v2/entries/en-gb/" + word);
            var result = JsonConvert.DeserializeObject<OxfordObject>(response);
            var list = result.results.Select(s =>
                  s.lexicalEntries.SelectMany(w => w.entries.SelectMany(we => we.senses.Select(wer => new
                  {
                      examples = wer.examples?.Select(y => y.text) ?? new List<string>(),
                      definitions = wer.definitions
                  }))));
            return Ok(list);
        }

        private async Task InsertWordsToDb()
        {
            var lines = System.IO.File.ReadAllLines(@"C:\Projects\WordGame\files\words.txt");
            using (var con = _sqlConnectionFactory.GetConnection())
            {
                foreach (var line in lines)
                {
                    var selectQ = "SELECT id from words where word=@word";
                    var exist = await con.QueryFirstOrDefaultAsync<int?>(selectQ, new
                    {
                        word = line
                    });
                    if (!exist.HasValue)
                    {
                        var q = "INSERT INTO public.words(word) VALUES (@word);";
                        await con.ExecuteAsync(q, new
                        {
                            word = line
                        });
                    }
                }
            }
        }
    }
}
