using quote_service.Models;

namespace quote_service.Services;

public interface IQuoteHistoryService
{
    Task<QuoteCompareResponse> CompareAndSaveAsync(QuoteCompareRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<QuoteHistoryResponse>> GetRecentQuotesAsync(CancellationToken cancellationToken);
    Task<QuoteHistoryResponse?> GetQuoteAsync(int id, CancellationToken cancellationToken);
}
