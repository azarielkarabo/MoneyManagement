using Money.Management.Entities;
using Money.Management.Permissions;
using Money.Management.Services.Budgets;
using Money.Management.Services.Budgets.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Money.Management.Services
{
    public class BudgetInsightsAppService : ApplicationService, IBudgetInsightsAppService
    {
        private const string AnthropicMessagesUrl = "https://api.anthropic.com/v1/messages";
        private const string AnthropicVersion = "2023-06-01";
        private const string DefaultModel = "claude-sonnet-4-5";

        private static readonly HttpClient HttpClient = new();

        private readonly IRepository<Budget, Guid> _budgetRepository;
        private readonly IRepository<BudgetItem, Guid> _budgetItemRepository;
        private readonly IConfiguration _configuration;

        public BudgetInsightsAppService(
            IRepository<Budget, Guid> budgetRepository,
            IRepository<BudgetItem, Guid> budgetItemRepository,
            IConfiguration configuration)
        {
            _budgetRepository = budgetRepository;
            _budgetItemRepository = budgetItemRepository;
            _configuration = configuration;
        }

        [Authorize(MoneyManagementPermissions.Budgets.Default)]
        public async Task<BudgetInsightsDto> GetMonthlySummaryAsync(int year, int month)
        {
            var apiKey = _configuration["AI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new UserFriendlyException(
                    "AI insights aren't configured yet. Add an Anthropic API key under the \"AI\" section in appsettings.");

            var budget = await _budgetRepository.FindAsync(
                b => b.UserId == CurrentUser.Id.Value && b.Year == year && b.Month == month);

            if (budget == null)
                throw new UserFriendlyException($"No budget found for {month}/{year}.");

            var items = await _budgetItemRepository.GetListAsync(i => i.BudgetId == budget.Id);

            var prompt = BuildPrompt(budget, items);
            var summary = await CallAnthropicAsync(prompt, apiKey);

            return new BudgetInsightsDto
            {
                Summary = summary,
                GeneratedAt = DateTime.UtcNow
            };
        }

        private static string BuildPrompt(Budget budget, List<BudgetItem> items)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a personal finance assistant. Write a short, friendly, plain-text summary " +
                          "(4-6 sentences, no markdown headers) of this person's spending for the month below. " +
                          "Call out anything over budget, praise good habits, and give one concrete, actionable suggestion.");
            sb.AppendLine();
            sb.AppendLine($"Salary: {budget.Salary:C}");
            sb.AppendLine($"Month/Year: {budget.Month}/{budget.Year}");
            sb.AppendLine("Budget items (name, type, planned, actual):");

            foreach (var item in items)
            {
                sb.AppendLine(
                    $"- {item.Name} ({item.Type}): planned {item.Amount:C}, actual {(item.ActualAmount.HasValue ? item.ActualAmount.Value.ToString("C") : "not yet recorded")}");
            }

            return sb.ToString();
        }

        private async Task<string> CallAnthropicAsync(string prompt, string apiKey)
        {
            var model = _configuration["AI:Model"];
            if (string.IsNullOrWhiteSpace(model))
                model = DefaultModel;

            var requestBody = new
            {
                model,
                max_tokens = 400,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, AnthropicMessagesUrl);
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("anthropic-version", AnthropicVersion);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await HttpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new UserFriendlyException($"AI insights request failed ({(int)response.StatusCode}): {responseBody}");

            using var doc = JsonDocument.Parse(responseBody);
            var text = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            return text ?? string.Empty;
        }
    }
}
