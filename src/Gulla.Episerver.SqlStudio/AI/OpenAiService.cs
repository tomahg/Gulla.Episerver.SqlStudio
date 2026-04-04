using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gulla.Episerver.SqlStudio.AI
{
    public class OpenAiService
    {
        private readonly string _endpointCompletions = "https://api.openai.com/v1/chat/completions";
        private readonly string _aiGenerateSystemTextIntroduction = "You are an SQL expert. Consider the following MSSQL tables from Episerver/Optimizely CMS, with their properties. There might be some tables not part of the default installation.\r\n\r\nAnswer every question with SQL query only. Your response should not include anything other than the SQL query.\r\n\r\n";
        private readonly string _aiExplainSystemTextIntroduction = "You are an SQL expert. Consider the following MSSQL query and the tables from Episerver/Optimizely CMS, with their properties. There might be some tables not part of the default installation.\r\n\r\nPlease explain the SQL query. Each line in your response should start with two dashes --. Do not use markdown formatting for any SQL snippets.\r\n\r\n";

        public async Task<string> GenerateSql(string prompt, string tablesMetaData, string contentTypeNames, string apiKey, string model, double temperature)
        {
            var systemPrompt = _aiGenerateSystemTextIntroduction + tablesMetaData + contentTypeNames;
            return await SendPrompt(prompt, systemPrompt, apiKey, model, temperature);
        }

        public async Task<string> ExplainSql(string prompt, string tablesMetaData, string contentTypeNames, string apiKey, string model, double temperature)
        {
            var systemPrompt = _aiExplainSystemTextIntroduction + tablesMetaData + contentTypeNames;
            return await SendPrompt(prompt, systemPrompt, apiKey, model, temperature);
        }

        private async Task<string> SendPrompt(string userPrompt, string systemPrompt, string apiKey, string model, double temperature)
        {
            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _endpointCompletions);
            var messages = new[] { new { role = "system", content = systemPrompt }, new { role = "user", content = userPrompt } };
            var body = JsonSerializer.Serialize(new { messages = messages, model = model, temperature = temperature });
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            var response = await httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var error) && error.TryGetProperty("message", out var errorMsg))
            {
                var errorMessage = errorMsg.GetString();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    throw new Exception(errorMessage);
                }
            }

            if (root.TryGetProperty("choices", out var choices) &&
                choices.GetArrayLength() > 0 &&
                choices[0].TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var contentProp))
            {
                var text = contentProp.GetString();
                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }

            return "Error generating explaination";
        }
    }
}
