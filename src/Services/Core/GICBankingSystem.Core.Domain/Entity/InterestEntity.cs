using GICBankingSystem.Core.Domain.Exceptions;

namespace GICBankingSystem.Core.Domain.Models;

public class InterestEntity
{
    public string RuleId { get; set; }
    public DateTime EffectiveDate { get; set; }
    public decimal Rate { get; set; }
    

    public void Add(string ruleId, DateTime effectiveDate, decimal rate)
    {
        if (string.IsNullOrEmpty(ruleId))
            throw new ArgumentNullException(nameof(ruleId));
        if (effectiveDate == DateTime.MinValue)
            throw new ArgumentNullException(nameof(effectiveDate));
        if (rate <= 0 || rate >= 100)
            throw new DomainException($"wrong rate {rate}");

        RuleId = ruleId;
        EffectiveDate = effectiveDate;
        Rate = rate;
    }

}
