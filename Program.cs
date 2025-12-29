using System.Text;
using APIGigaChatImage_pr11_permilka.Models.Response;
using Newtonsoft.Json;

namespace APIGigaChatImage_pr11_permilka
{
    class Program
    {
        static string ClientId = "***";
        static string AuthorizationKey = "***";
        public static async Task<string> GetToken(string rqUID, string bearer)
        {
            string ReturnToken = null;
            string Url = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";
            using (HttpClientHandler Handler = new HttpClientHandler())
            {
                Handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;
                using (HttpClient Clien = new HttpClient(Handler))
                {
                    HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, Url);
                    Request.Headers.Add("Accept", "application/json");
                    Request.Headers.Add("RqUID", rqUID);
                    Request.Headers.Add("Authorization", $"Bearer {bearer}");
                    var Data = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("scope", "GIGACHAT_API_PERS")
                    };
                    Request.Content = new FormUrlEncodedContent(Data);
                    HttpResponseMessage Response = await Clien.SendAsync(Request);
                    if (Response.IsSuccessStatusCode)
                    {
                        string ResponseContent = await Response.Content.ReadAsStringAsync();
                        ResponseToken Token = JsonConvert.DeserializeObject<ResponseToken>(ResponseContent);
                        ReturnToken = Token.access_token;
                    }
                }
            }
            return ReturnToken;
        }

        public static async Task<string> GenerateImage(string token, string prompt)
        {
            string imageUrl = null;
            string url = "https://gigachet.devices.sberbank.ru/api/v1/chat/completions";

            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;
                using (HttpClient client = new HttpClient(handler))
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Headers.Add("Accept", "application/json");
                    request.Headers.Add("Authorization", $"Bearer {token}");
                    var requestBody = new
                    {
                        model = "GigaChat",
                        messages = new[]
                        {
                            new
                            {
                                role = "system",
                                content = "Я -- Василий Кандинский"
                            },
                            new
                            {
                                role = "user",
                                content = prompt
                            }
                        }
                    };
                    string jsonContent = JsonConvert.SerializeObject(requestBody);
                    request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Запрос успешно отправлен!");
                        Console.WriteLine($"Ответ: {responseContent}");
                        imageUrl = responseContent;
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка: {response.StatusCode}");
                        Console.WriteLine(await response.Content.ReadAsStringAsync());
                    }
                }
            }

            return imageUrl;
        }

        static async Task Main(string[] args)
        {
            string token = await GetToken(ClientId, AuthorizationKey);

            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Токен успешно получен!");
                string prompt = "Нарисуй розового кота";
                Console.WriteLine($"Отправляем промпт: {prompt}");
                string result = await GenerateImage(token, prompt);
                if (!string.IsNullOrEmpty(result))
                {
                    Console.WriteLine("Изображение успешно сгенерировано!");
                }
                else
                {
                    Console.WriteLine("Не удалось сгенерировать изображение.");
                }
            }
            else
            {
                Console.WriteLine("Не удалось получить токен.");
            }
        }
    }
}