using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoviCode.WalletService.Configuration;
using NoviCode.WalletService.Data;
using NoviCode.WalletService.Models;
using NoviCode.WalletService.Strategies;

namespace NoviCode.WalletService.Controllers;

[ApiController]
    [Route("api/[controller]")]
    public class WalletsController : ControllerBase
    {
        private readonly MyDbContext _dbContext;
        private readonly IStrategyFactory _strategyFactory;

        public WalletsController(MyDbContext dbContext, IStrategyFactory strategyFactory)
        {
            _dbContext = dbContext;
            _strategyFactory = strategyFactory;
        }

        // POST: api/wallets
        [HttpPost]
        public async Task<IActionResult> CreateWallet([FromBody] CreateWalletDto dto)
        {
            var wallet = new Wallet
            {
                Balance = dto.InitialBalance,
                Currency = dto.Currency
            };

            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWallet), new { id = wallet.Id }, wallet);
        }

        // GET: api/wallets/{id}?currency=XXX
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWallet(long id, [FromQuery] string? currency)
        {
            var wallet = await _dbContext.Wallets.FindAsync(id);
            if (wallet == null)
                return NotFound();

            // If no currency conversion is requested, return the wallet as-is.
            if (string.IsNullOrEmpty(currency) || currency.Equals(wallet.Currency, StringComparison.OrdinalIgnoreCase))
            {
                return Ok(wallet);
            }

            // Retrieve the latest exchange rates from the database.
            var latestDate = await _dbContext.ExchangeRates.MaxAsync(r => r.Date);
            var rates = await _dbContext.ExchangeRates.Where(r => r.Date == latestDate).ToListAsync();

            decimal ConvertBalance(decimal balance, string fromCurrency, string toCurrency)
            {
                if (fromCurrency.Equals("EUR", StringComparison.OrdinalIgnoreCase))
                {
                    var toRate = rates.FirstOrDefault(r => r.Currency == toCurrency);
                    if (toRate == null)
                        throw new Exception($"Missing rate for {toCurrency}");
                    return balance * toRate.Rate;
                }
                else
                {
                    var fromRate = rates.FirstOrDefault(r => r.Currency == fromCurrency);
                    var toRate = rates.FirstOrDefault(r => r.Currency == toCurrency);
                    if (fromRate == null || toRate == null)
                        throw new Exception($"Missing rate for {fromCurrency} or {toCurrency}");

                    var balanceInEur = balance / fromRate.Rate;
                    return balanceInEur * toRate.Rate;
                }
            }

            var convertedBalance = ConvertBalance(wallet.Balance, wallet.Currency, currency);
            var responseDto = new WalletBalanceDto(wallet.Id, convertedBalance, currency);
            return Ok(responseDto);
        }

        // POST: api/wallets/{id}/adjustbalance?amount=XX&currency=YY&strategy=ZZ
        [HttpPost("{id}/adjustbalance")]
        public async Task<IActionResult> AdjustWalletBalance(
            long id,
            [FromQuery] decimal amount,
            [FromQuery] string currency,
            [FromQuery] string strategy)
        {
            if (amount <= 0)
                return BadRequest("Amount must be positive.");

            var wallet = await _dbContext.Wallets.FindAsync(id);
            if (wallet == null)
                return NotFound();

            // Retrieve the latest exchange rates.
            var latestDate = await _dbContext.ExchangeRates.MaxAsync(r => r.Date);
            var rates = await _dbContext.ExchangeRates.Where(r => r.Date == latestDate).ToListAsync();

            decimal ConvertAmount(decimal amt, string fromCurrency, string toCurrency)
            {
                if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
                    return amt;

                if (fromCurrency.Equals("EUR", StringComparison.OrdinalIgnoreCase))
                {
                    var toRate = rates.FirstOrDefault(r => r.Currency == toCurrency);
                    if (toRate == null)
                        throw new Exception($"Missing rate for {toCurrency}");
                    return amt * toRate.Rate;
                }
                else
                {
                    var fromRate = rates.FirstOrDefault(r => r.Currency == fromCurrency);
                    var toRate = rates.FirstOrDefault(r => r.Currency == toCurrency);
                    if (fromRate == null || toRate == null)
                        throw new Exception($"Missing rate for {fromCurrency} or {toCurrency}");

                    var amtInEur = amt / fromRate.Rate;
                    return amtInEur * toRate.Rate;
                }
            }

            var convertedAmount = ConvertAmount(amount, currency, wallet.Currency);
            var balanceStrategy = _strategyFactory.GetStrategy(strategy);

            try
            {
                balanceStrategy.Execute(wallet, convertedAmount);
                await _dbContext.SaveChangesAsync();
                return Ok(wallet);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }