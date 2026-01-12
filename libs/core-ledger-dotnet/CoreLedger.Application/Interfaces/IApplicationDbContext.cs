using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#pragma warning disable CS0618 // Type or member is obsolete - Fund is deprecated but still used for legacy support

namespace CoreLedger.Application.Interfaces;

/// <summary>
///     Interface for the application database context exposing DbSets for direct use in handlers.
/// </summary>
public interface IApplicationDbContext
{
    DatabaseFacade Database { get; }
    DbSet<AccountType> AccountTypes { get; }
    DbSet<Account> Accounts { get; }
    DbSet<CoreJob> CoreJobs { get; }
    DbSet<Fund> Funds { get; }
    DbSet<Fundo> Fundos { get; }
    DbSet<FundoClasse> FundoClasses { get; }
    DbSet<FundoSubclasse> FundoSubclasses { get; }
    DbSet<FundoTaxa> FundoTaxas { get; }
    DbSet<FundoTaxaPerformance> FundoTaxasPerformance { get; }
    DbSet<FundoPrazo> FundoPrazos { get; }
    DbSet<FundoPrazoExcecao> FundoPrazoExcecoes { get; }
    DbSet<Indexador> Indexadores { get; }
    DbSet<HistoricoIndexador> HistoricosIndexadores { get; }
    DbSet<Security> Securities { get; }
    DbSet<TransactionStatus> TransactionStatuses { get; }
    DbSet<TransactionType> TransactionTypes { get; }
    DbSet<TransactionSubType> TransactionSubTypes { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<TransactionIdempotency> TransactionIdempotencies { get; }
    DbSet<TransactionCreatedOutboxMessage> TransactionCreatedOutboxMessages { get; }
    DbSet<User> Users { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Calendario> Calendarios { get; }
    DbSet<Instituicao> Instituicoes { get; }
    DbSet<ClassificacaoAnbima> ClassificacoesAnbima { get; }
    DbSet<FundoVinculo> FundoVinculos { get; }
    DbSet<FundoParametrosFIDC> FundoParametrosFIDC { get; }
    DbSet<FundoParametrosCota> FundoParametrosCota { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
