using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml.XPath;
using Dapper;
using FlashCards.Data;
using Microsoft.AspNetCore.Mvc;
using FlashCards.Models;
using HtmlAgilityPack;
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

        [HttpGet("/get-random")]
        public async Task<IActionResult> GetRandomWord()
        {
            using (var con = _sqlConnectionFactory.GetConnection())
            {
                var q = "SELECT word FROM words ORDER BY random() LIMIT 1";
                return Ok(await con.QueryFirstOrDefaultAsync<WordDto>(q));
            }
        }

        [HttpGet("/get-examples")]
        public async Task<IActionResult> GetWordExamples([FromQuery]string word)
        {
            var column = "senses";
            var result = await FetchValueFromDb<IList<WordSenses>>(word, column);
            if (result != null)
                return Ok(result);
            result = await FetchExamplesFromOxford(word);
            await AddResultToDb(result, word, column).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("/get-translations")]
        public async Task<IActionResult> GetTranslation([FromQuery]string word)
        {
            var column = "translations";
            var result = await FetchValueFromDb<IList<TranslationDto>>(word, column);
            if (result != null)
                return Ok(result);
            result = await FetchTranslationsFromTureng(word);
            await AddResultToDb(result, word, column);
            return Ok(result);
        }

        [HttpPost("add-new")]
        public async Task<IActionResult> AddNewWord([FromBody]AddWordRequest request)
        {
            if (string.IsNullOrEmpty(request?.Word))
                return BadRequest("No word");
            using (var con = _sqlConnectionFactory.GetConnection())
            {
                var selectQ = "SELECT id from words where word=@word";
                var exist = await con.QueryFirstOrDefaultAsync<int?>(selectQ, new
                {
                    word = request.Word
                });
                if (exist.HasValue)
                    return BadRequest("already exist");
                var insertQ = "INSERT INTO public.words(word) VALUES (@word);";
                await con.ExecuteAsync(insertQ, new
                {
                    word = request.Word
                });
            }

            return Ok();
        }

        private async Task<IList<WordSenses>> FetchExamplesFromOxford(string word)
        {
            var client = _httpClientFactory.CreateClient("oxford");
            client.DefaultRequestHeaders.Add("app_id", "b5f71d5e");
            client.DefaultRequestHeaders.Add("app_key", "ebb0797e40dd8addbf8b03e7633399e9");
            var response = await client.GetStringAsync("https://od-api.oxforddictionaries.com/api/v2/entries/en-gb/" + word);
            var deserializedObject = JsonConvert.DeserializeObject<OxfordServiceResponse>(response);
            var result = deserializedObject.results.SelectMany(s =>
                s.lexicalEntries.SelectMany(w => w.entries.SelectMany(we => we.senses.Select(wer => new WordSenses
                {
                    Examples = wer.examples?.Select(y => y.text).ToList() ?? new List<string>(),
                    Definitions = wer.definitions ?? new List<string>()
                })))).ToList();
            return result;
        }

        private async Task<T> FetchValueFromDb<T>(string word, string column)
        {
            using (var con = _sqlConnectionFactory.GetConnection())
            {
                var translationsStr = await con.QueryFirstOrDefaultAsync<string>($"SELECT {column} FROM words WHERE word=@word", new { word, column });
                if (!string.IsNullOrEmpty(translationsStr))
                    return JsonConvert.DeserializeObject<T>(translationsStr);
            }

            return default;
        }

        private async Task<IList<TranslationDto>> FetchTranslationsFromTureng(string word)
        {
            var url = $"http://tureng.com/en/turkish-english/{HttpUtility.UrlEncode(word)}";
            var client = _httpClientFactory.CreateClient("tureng");
            var strResult = await client.GetStringAsync(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(strResult);
            var translationsNodes = htmlDocument.DocumentNode.SelectNodes(XPathExpression.Compile("//table[@id='englishResultsTable']/tr/td[@lang='tr']"));
            var categoriesNodes = htmlDocument.DocumentNode.SelectNodes(XPathExpression.Compile("//table[@id='englishResultsTable']/tr/td[@class='hidden-xs']"));
            var result = new List<TranslationDto>();
            for (var i = 0; i < translationsNodes.Count; i++)
            {
                result.Add(new TranslationDto
                {
                    Category = categoriesNodes[i].InnerText,
                    Translation = translationsNodes[i].InnerText
                });
            }

            return result;
        }

        private async Task AddResultToDb<T>(T result, string word, string column)
        {
            using (var con = _sqlConnectionFactory.GetConnection())
            {
                await con.ExecuteAsync($"UPDATE words SET {column} = CAST(@value AS json) WHERE word=@word", new
                {
                    value = JsonConvert.SerializeObject(result),
                    word,
                    column
                });
            }
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
