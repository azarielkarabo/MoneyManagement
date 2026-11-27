using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Money.Management.Entities;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Money.Management.Data;

/// <summary>
/// Before top-ups had their own table, each one was only written into the loan's notes as
/// "[yyyy-MM-dd] Top-up R 2,500.00 at 30% — note". This turns those lines into
/// <see cref="CreditTopUp"/> rows, once per loan (loans that already have top-up rows are skipped).
/// </summary>
public class CreditTopUpBackfillDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private static readonly Regex TopUpLine = new(
        @"^\[(?<date>\d{4}-\d{2}-\d{2})\] Top-up R\s?(?<amount>[\d\s, ]+(?:[.,]\d{1,2})?) at (?<rate>[\d.,]+)%(?:\s+—\s+(?<note>.*))?$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    private readonly IRepository<Credit, Guid> _creditRepository;
    private readonly IRepository<CreditTopUp, Guid> _topUpRepository;
    private readonly IGuidGenerator _guidGenerator;

    public CreditTopUpBackfillDataSeedContributor(
        IRepository<Credit, Guid> creditRepository,
        IRepository<CreditTopUp, Guid> topUpRepository,
        IGuidGenerator guidGenerator)
    {
        _creditRepository = creditRepository;
        _topUpRepository = topUpRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var credits = await _creditRepository.GetListAsync(c => c.Notes != null && c.Notes.Contains("Top-up R"));
        if (credits.Count == 0) return;

        var creditIds = credits.Select(c => c.Id).ToList();
        var alreadyDone = (await _topUpRepository.GetListAsync(t => creditIds.Contains(t.CreditId)))
            .Select(t => t.CreditId)
            .ToHashSet();

        var rows = new List<CreditTopUp>();
        foreach (var credit in credits.Where(c => !alreadyDone.Contains(c.Id)))
        {
            foreach (Match m in TopUpLine.Matches(credit.Notes!.Replace("\r", "")))
            {
                if (!DateTime.TryParseExact(m.Groups["date"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date)) continue;
                if (!TryParseMoney(m.Groups["amount"].Value, out var amount) || amount <= 0) continue;
                if (!TryParseMoney(m.Groups["rate"].Value, out var rate)) continue;

                var note = m.Groups["note"].Success ? m.Groups["note"].Value.Trim() : null;
                rows.Add(new CreditTopUp(
                    _guidGenerator.Create(),
                    credit.Id,
                    amount,
                    rate,
                    Math.Round(amount * (rate / 100), 2),
                    date,
                    outstandingAfter: null,
                    string.IsNullOrWhiteSpace(note) ? null : note));
            }
        }

        if (rows.Count > 0)
        {
            await _topUpRepository.InsertManyAsync(rows, autoSave: true);
        }
    }

    // Accepts "2,500.00", "2 500,00" and "2500".
    private static bool TryParseMoney(string raw, out decimal value)
    {
        var s = raw.Replace(" ", "").Replace(" ", "").Trim();
        var lastSep = Math.Max(s.LastIndexOf('.'), s.LastIndexOf(','));
        if (lastSep >= 0 && s.Length - lastSep - 1 <= 2)
        {
            s = s[..lastSep].Replace(",", "").Replace(".", "") + "." + s[(lastSep + 1)..];
        }
        else
        {
            s = s.Replace(",", "").Replace(".", "");
        }
        return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }
}
