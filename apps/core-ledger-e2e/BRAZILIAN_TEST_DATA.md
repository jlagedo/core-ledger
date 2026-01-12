# Brazilian Financial Test Data Reference

## Real Securities (B3 2025)

### Most Traded Stocks

| Ticker | Company | ISIN | Sector | Currency |
|--------|---------|------|--------|----------|
| PETR4 | Petróleo Brasileiro (Petrobras) | BR0155900909 | Energy | BRL |
| VALE3 | Vale S/A | BR0110673005 | Mining | BRL |
| ITUB4 | Itaú Unibanco Holding | BR0120234086 | Banking | BRL |
| BBDC3 | Banco Bradesco | BR0156005917 | Banking | BRL |
| BBAS3 | Banco do Brasil | BR0156900408 | Banking | BRL |

### Test Prices (Approximate - 2025)
- PETR4: 28.50 BRL
- VALE3: 62.75 BRL
- ITUB4: 35.20 BRL
- BBDC3: 32.50 BRL (estimated)

## Real Brazilian Funds (FIIs)

### Largest by AUM (H1 2025)

| Ticker | Fund Name | AUM (R$ B) | Type | Sector |
|--------|-----------|-----------|------|--------|
| XPML11 | XP Malls | 6.37 | Brick | Retail |
| BTLG11 | BTG Pactual Logística | 5.22 | Brick | Logistics |
| KNRI11 | Kinea Renda Imobiliária | 4.61 | Brick | Mixed |
| VISC11 | Vinci Shopping Centers | 3.56 | Brick | Retail |

### FII Types

1. **FII de Tijolo** - Real estate investments
   - Shopping centers, offices, logistics warehouses
   - Monthly/quarterly dividend distribution

2. **FII de Papel** - Fixed income instruments
   - CRI (Certificado de Recebíveis Imobiliários)
   - LCI (Letras de Crédito Imobiliário)

3. **FII de Fundos (FoF)** - Fund of funds
   - Invest in other FIIs

4. **FII Híbrido** - Mixed assets
   - Combination of brick and paper

## Brazilian Financial Institutions

### Major Banks (By Customer Base)

| Institution | CNPJ | Type | Customers |
|-------------|------|------|-----------|
| Caixa Econômica Federal | 00.360.305/0001-04 | Public Bank | 154M |
| Banco do Brasil | 00.000.191/0001-90 | Public Bank | 100M+ |
| Bradesco | 60.746.948/0001-12 | Private Bank | 70M+ |
| Itaú Unibanco | 17.191.814/0001-21 | Private Bank | 40M+ |

## Brazilian Economic Indices

### Key Rates (January 2026)

| Index | Code | Value | Type | Update |
|-------|------|-------|------|--------|
| Selic | SELIC | 10.65% | Base Rate | Weekly |
| CDI | CDI | 10.40% | Interbank | Daily |
| IPCA | IPCA | 4.50% | Inflation | Monthly |
| IGPM | IGPM | 5.20% | Price Index | Monthly |

### Index Purposes

- **Selic**: Central Bank base interest rate, affects all other rates
- **CDI**: Used to price fixed income investments, especially overnight operations
- **IPCA**: Official inflation index, used to adjust indexed investments
- **IGPM**: Alternative inflation measure, used in some indexed bonds

## Transaction Patterns

### Settlement Dates (Brazilian Standard)
- **D+0**: Same-day settlement (rare)
- **D+1**: Next business day (bonds)
- **D+2**: Two business days (stocks) ⭐ **STANDARD**

### Transaction Types

1. **Purchase** - Buy securities
   - Debit account, credit securities
   - Amount = Quantity × Price

2. **Sale** - Sell securities
   - Credit account, debit securities
   - Gross proceeds, before fees

3. **Dividend Receipt**
   - Per share dividend payment
   - Credited to cash account

4. **Rights Distribution**
   - New securities from corporate actions
   - Typically free distribution

5. **Bonus Shares**
   - Free additional shares
   - From profit distribution

## Test Data Usage Rules

### CNPJ Format
- **Valid Format**: XX.XXX.XXX/YYYY-ZZ
- **Parts**:
  - XX.XXX.XXX: Root number (8 digits)
  - YYYY: Registration branch
  - ZZ: Sequential number

### ISIN Code Format
- **Pattern**: BR + 9 characters
- **Example**: BR0155900909 (PETR4)
- **Standard**: ISO 6166

### Stock Code Format
- **Digits Only**: Numeric ticker (PETR4, VALE3)
- **Suffix Meanings**:
  - 3 = Ordinary Shares (ON)
  - 4 = Preferred Shares (PN)
  - 5/6 = Class B/C shares
  - 11 = FII shares

## Regulatory Context

### Key Regulations
- **CVM 175/2022**: FII regulations
- **Banco Central**: Selic and monetary policy
- **ANBIMA**: Investment guidelines
- **B3**: Market operations

### Typical Restrictions
- Fund must have minimum AUM
- FIIs must distribute 95% of profits
- Monthly or quarterly distribution
- Minimum fund maturity period

## Mock Data Generation

### Random CNPJ Generation (for testing)
```
Format: XXXXXXXXXXXXXXXX
Rule: Use actual format XX.XXX.XXX/YYYY-ZZ
Example: 17.191.814/0001-21
```

### Random Security Codes (for testing)
```
Pattern: TEST + Date.now()
Example: TEST1704067200000
Keep under 20 characters for ticker field
```

### Real Fund Codes (Production)
- Always use actual B3 codes (XPML11, BTLG11, etc.)
- For new funds, verify registration with CVM
- Check fund prospectus for investor requirements

## Test Data Maintenance

### Update Frequency
- **Securities Prices**: Daily (external data)
- **Economic Indices**: Monthly (for IPCA, IGPM)
- **Interest Rates**: Weekly (for Selic)
- **Fund Information**: Quarterly

### Data Sources
1. **B3** (www.b3.com.br) - Official exchange data
2. **Banco Central** - Interest rates and inflation
3. **ANBIMA** - Fund standards and regulations
4. **Fund Manager Websites** - Current NAV and holdings

## Performance Test Data

### Large Portfolio Scenarios
- 1000+ positions in fund
- 500+ transactions per month
- Multiple currencies (BRL, USD, EUR)
- 5+ years of historical data

### Volume Testing
- 10,000+ funds in system
- 50,000+ securities
- 1M+ transactions
- 100M+ index history records
