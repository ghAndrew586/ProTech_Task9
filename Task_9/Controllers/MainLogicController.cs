using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.RegularExpressions;
using Task_9.Models;
using Task_9.SortingAlgorithms;

namespace Task_9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainLogicController : ControllerBase
    {
        private static readonly IConfiguration jsonConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(jsonConfig.GetSection("Settings:ParallelLimit").Get<int>());

        [HttpGet]
        public ActionResult Get(string inputLine, int sortOption)
        {
            if (!semaphore.Wait(0))
            {
                return StatusCode(503, "Слишком много запросов на сервер!");
            }

            try
            {
                string badRequestLine;

                ResultData resultData = new ResultData();

                //Checking concurrent requests
                //Thread.Sleep(5000);

                if (!Regex.IsMatch(inputLine, "^[a-z]+$"))
                {
                    if (Regex.Replace(inputLine, " ", "") != "")
                    {
                        badRequestLine = "В строке должны быть только латинские буквы в нижнем регистре! Неподходящие символы: " +
                            Regex.Replace(Regex.Replace(inputLine, "[a-z]", ""), " ", " 'Пробел' ");
                    }
                    else
                    {
                        badRequestLine = "Строка не должна быть пустой!";
                    }
                    return BadRequest(badRequestLine);
                }

                if (jsonConfig.GetSection("Settings:BlackList").Get<string[]>().Any(str => str == inputLine))
                {
                    badRequestLine = $"'{inputLine}' находится в черном списке: ";
                    foreach (string str in jsonConfig.GetSection("Settings:BlackList").Get<string[]>())
                    {
                        badRequestLine += str + ", ";
                    }
                    return BadRequest(badRequestLine);
                }

                char[] mainLine = inputLine.ToCharArray();

                Array.Reverse(mainLine);
                string resultLine;
                if (mainLine.Length % 2 != 0)
                {
                    resultLine = new string(mainLine);
                    Array.Reverse(mainLine);
                    resultLine += new string(mainLine);

                    resultData.ResultLine = resultLine;

                }
                else
                {
                    var lastSegment = new ArraySegment<char>(mainLine, 0, mainLine.Length / 2);
                    var firstSegment = new ArraySegment<char>(mainLine, mainLine.Length / 2, mainLine.Length / 2);
                    resultLine = String.Join("", firstSegment) + (String.Join("", lastSegment));
                    resultData.ResultLine = resultLine;
                    Array.Reverse(mainLine);
                }

                resultData.CharsAmounts = new Dictionary<char, int>();
                foreach (char letter in mainLine)
                {
                    resultData.CharsAmounts[letter] = resultLine.Count(lt => lt == letter);
                }

                string maxLine = "";

                foreach (Match match in Regex.Matches(resultLine, "[aeiouy].*[aeiouy]"))
                {
                    if (maxLine.Length < match.Value.Length)
                    {
                        maxLine = match.Value;
                    }
                }

                resultData.LongSubline = maxLine;

                if (sortOption != 1 && sortOption != 2)
                {
                    return BadRequest("1 - quick sort; 2 - tree sort!");
                }

                char[] resultLineChars = resultLine.ToCharArray();

                Quicksort quicksort = new Quicksort();
                Treesort treesort = new Treesort();

                if (sortOption == 1)
                {
                    resultData.SortResultLine = new string(quicksort.QuicksortLogic(resultLineChars, 0, resultLineChars.Length - 1));
                }
                else if (sortOption == 1)
                {
                    resultData.SortResultLine = new string(treesort.TreeSort(resultLineChars));
                }

                int delIndex;
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(jsonConfig.GetSection("RandomApi").Get<string>() + $"?min=0&max={resultLine.Length}&count=1");
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        delIndex = (streamReader.ReadToEnd()[1] - '0');
                    }
                }
                catch (WebException)
                {
                    Random rnd = new Random();
                    delIndex = rnd.Next(0, resultLine.Length);
                }

                resultData.ReduceLine = resultLine.Remove(delIndex, 1);

                return Ok(resultData);
            }
            finally 
            {
                semaphore.Release();
            }
        }
    }
}
