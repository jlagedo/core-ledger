# 📘 **B3 Instrument Type → SecurityType Enum Mapping**

| **B3 Instrument Type** | **Description** | **Enum Value** | **SecurityType** |
|------------------------|-----------------|----------------|------------------|
| **Ações (Stocks)** | PETR4, VALE3, etc. | 1 | **Equity** |
| **Units** | e.g., SANB11 | 1 | **Equity** |
| **BDRs** | Brazilian Depositary Receipts | 15 | **Receipt** |
| **ETFs** | BOVA11, IVVB11 | 6 | **ETF** |
| **FIIs (REIT‑like)** | Fundos Imobiliários | 7 | **REIT** |
| **Mutual Funds / FIC / FIM** | Fund shares | 14 | **Fund** |
| **Tesouro Direto (Gov Bonds)** | IPCA+, Prefixado, Selic | 2 | **Bond** |
| **Debêntures** | Corporate bonds | 2 | **Bond** |
| **CRI / CRA** | Securitized credit | 2 | **Bond** |
| **Commercial Papers** | Notas Comerciais | 2 | **Bond** |
| **CDB / RDB** | Bank deposits | 4 | **MoneyMarket** |
| **Repos (Compromissadas)** | Collateralized repos | 4 | **MoneyMarket** |
| **Cash Position** | Conta corrente | 3 | **Cash** |
| **Futures (Futuro)** | DOL, WIN, DI1, CCM, ICF | 10 | **Future** |
| **Mini Futures** | WDO, WIN | 10 | **Future** |
| **Options on Equities** | PETR4 Call/Put | 11 | **OptionOnEquity** |
| **Options on Futures** | Options on DOL, DI, etc. | 12 | **OptionOnFuture** |
| **Options on FX** | USD/BRL options | 12 | **OptionOnFuture** (derivative on future) |
| **Forward / Termo** | Termo de Ações, Termo de Dólar | 13 | **Forward** |
| **Swaps** | DI x Pré, IPCA x CDI | 8 | **Derivative** |
| **NDF / FRA** | Non‑deliverable forwards | 13 | **Forward** |
| **FX Spot (Câmbio Pronto)** | USD/BRL spot | 16 | **FX** |
| **Commodities Spot** | Ouro, boi, milho spot | 17 | **Commodity** |
| **Commodity Futures** | CCM, ICF, BGI | 10 | **Future** |
| **Commodity Options** | Options on CCM, ICF | 12 | **OptionOnFuture** |
| **Indexes (Índices)** | IBOV, IBrX, SMLL | 18 | **Index** |
| **COE (Structured Notes)** | Notas estruturadas | 9 | **Hybrid** |
| **Hybrid Instruments** | Any multi‑asset structured product | 9 | **Hybrid** |

---

# 🎯 Notes for Implementation

### ✔ Futures vs Options on Futures  
B3 options on DI, DOL, IND, etc. are **OptionOnFuture**, not OptionOnEquity.

### ✔ FX  
- **Spot FX → FX**  
- **FX Futures → Future**  
- **FX Options → OptionOnFuture**  
- **FX Termo → Forward**

### ✔ Bonds  
Everything fixed‑income (public or private) → **Bond**.

### ✔ Funds  
All FIM, FIC, FIDC, FII‑FIM, etc. → **Fund**  
FIIs specifically → **REIT**.

### ✔ Derivatives  
Only **Swaps** and exotic structures go to **Derivative** (generic).