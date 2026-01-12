{{
  config(
    materialized='table',
    description='B3 instruments with generated human-readable names'
  )
}}

WITH staging AS (
  SELECT * FROM {{ ref('stg_b3_instruments') }}
),

instrument_names AS (
  SELECT
    *,

    CASE
      -- ===== FUTURES =====

      -- 1. MINI FUTURES (e.g., WDO, WIN)
      WHEN "MktNm" = 'FUTURE'
        AND is_mini_contract = TRUE
        AND expiration_month_name IS NOT NULL
      THEN 'Mini ' || COALESCE("AsstDesc", "Asst") || ' Futuro ' ||
           expiration_month_name || ' ' || expiration_year

      -- 2. STOCK FUTURES (e.g., Petrobras Futuro)
      WHEN "MktNm" = 'FUTURE'
        AND "SctyCtgyNm" = 'STOCK FUTURE'
        AND expiration_month_name IS NOT NULL
      THEN COALESCE("AsstDesc", "Asst") || ' Futuro ' ||
           expiration_month_name || ' ' || expiration_year

      -- 3. GENERIC FUTURES (Financial segment)
      WHEN "MktNm" = 'FUTURE'
        AND "SgmtNm" = 'FINANCIAL'
        AND expiration_month_name IS NOT NULL
      THEN COALESCE("AsstDesc", "Asst") || ' Futuro ' ||
           expiration_month_name || ' ' || expiration_year

      -- 4. AGRIBUSINESS FUTURES
      WHEN "MktNm" = 'FUTURE'
        AND "SgmtNm" = 'AGRIBUSINESS'
        AND expiration_month_name IS NOT NULL
      THEN COALESCE("AsstDesc", "Asst") || ' Futuro ' ||
           expiration_month_name || ' ' || expiration_year

      -- 5. OTHER FUTURES (catch-all for futures)
      WHEN "MktNm" = 'FUTURE'
        AND expiration_month_name IS NOT NULL
      THEN COALESCE("AsstDesc", "Asst") || ' Futuro ' ||
           expiration_month_name || ' ' || expiration_year


      -- ===== OPTIONS =====

      -- 6. EQUITY OPTIONS (Options on stocks)
      WHEN "MktNm" = 'EQUITY-DERIVATE'
        AND "SctyCtgyNm" = 'OPTION ON EQUITIES'
        AND option_type_pt IS NOT NULL
      THEN 'Opção de ' || COALESCE("TckrSymb", "Asst") || ' ' ||
           option_type_pt || ' ' || COALESCE(strike_formatted, '') || ' ' ||
           COALESCE(expiration_month_name || ' ' || expiration_year,
                    expiration_date_formatted, '')

      -- 7. INDEX OPTIONS (Options on indexes like Ibovespa)
      WHEN "MktNm" = 'EQUITY-DERIVATE'
        AND "SctyCtgyNm" = 'OPTION ON INDEX'
        AND option_type_pt IS NOT NULL
      THEN 'Opção de ' || COALESCE("AsstDesc", "Asst") || ' ' ||
           option_type_pt || ' ' || COALESCE(strike_formatted, '') || ' ' ||
           COALESCE(expiration_month_name || ' ' || expiration_year,
                    expiration_date_formatted, '')

      -- 8. OPTIONS ON FUTURES
      WHEN "MktNm" = 'OPTIONS ON FUTURE'
        AND option_type_pt IS NOT NULL
      THEN 'Opção de ' || COALESCE("AsstDesc", "Asst") || ' ' ||
           option_type_pt || ' ' || COALESCE(strike_formatted, '') || ' ' ||
           COALESCE(expiration_month_name || ' ' || expiration_year, '')

      -- 9. OPTIONS ON SPOT (e.g., Mini Dollar Options)
      WHEN "MktNm" = 'OPTIONS ON SPOT'
        AND option_type_pt IS NOT NULL
      THEN 'Opção de ' || COALESCE("AsstDesc", "Asst") || ' ' ||
           option_type_pt || ' ' || COALESCE(strike_formatted, '') || ' ' ||
           COALESCE(expiration_date_formatted, '')


      -- ===== EQUITIES =====

      -- 10. SHARES (Regular stocks)
      WHEN "MktNm" = 'EQUITY-CASH'
        AND "SctyCtgyNm" = 'SHARES'
      THEN COALESCE("CrpnNm", "AsstDesc", "Asst") || ' ' ||
           COALESCE("SpcfctnCd", '') || ' ' || "TckrSymb"

      -- 11. BDRs (Brazilian Depositary Receipts)
      WHEN "MktNm" = 'EQUITY-CASH'
        AND "SctyCtgyNm" = 'BDR'
      THEN COALESCE("CrpnNm", "AsstDesc", "Asst") || ' BDR ' ||
           COALESCE("SpcfctnCd", '')

      -- 12. UNITS (Stock units, e.g., SANB11)
      WHEN "MktNm" = 'EQUITY-CASH'
        AND "SctyCtgyNm" = 'UNIT'
      THEN COALESCE("CrpnNm", "AsstDesc", "Asst") || ' Unit ' || "TckrSymb"


      -- ===== ETFs & FUNDS =====

      -- 13. ETF EQUITIES
      WHEN "MktNm" = 'EQUITY-CASH'
        AND "SctyCtgyNm" = 'ETF EQUITIES'
      THEN COALESCE("CrpnNm", "AsstDesc") || ' (ETF)'

      -- 14. ETF FOREIGN INDEX
      WHEN "MktNm" = 'EQUITY-CASH'
        AND "SctyCtgyNm" = 'ETF FOREIGN INDEX'
      THEN COALESCE("CrpnNm", "AsstDesc") || ' (ETF Estrangeiro)'

      -- 15. FUNDS (Investment funds)
      WHEN "MktNm" = 'EQUITY-CASH'
        AND "SctyCtgyNm" = 'FUNDS'
      THEN 'Fundo ' || COALESCE("CrpnNm", "AsstDesc", "Asst")


      -- ===== FORWARDS & SWAPS =====

      -- 16. COMMON EQUITIES FORWARD (Termo)
      WHEN "MktNm" = 'EQUITY-DERIVATE'
        AND "SctyCtgyNm" = 'COMMON EQUITIES FORWARD'
      THEN COALESCE("UndrlygTckrSymb1", "Asst") || ' a Termo ' ||
           COALESCE(expiration_date_formatted, '')

      -- 17. FX SWAP
      WHEN "MktNm" = 'FORWARD'
        AND "SctyCtgyNm" = 'FX SWAP'
      THEN 'Swap ' || COALESCE("AsstDesc", "Asst") || ' ' ||
           COALESCE(expiration_date_formatted, '')


      -- ===== FIXED INCOME =====

      -- 18. FIXED INCOME TRADABLE INSTRUMENT
      WHEN "MktNm" = 'FIXED INCOME'
        AND "SctyCtgyNm" LIKE '%FIXED INCOME%'
      THEN COALESCE("AsstDesc", "CrpnNm", "Asst") || ' ' ||
           COALESCE(expiration_date_formatted, '')


      -- ===== OTHER INSTRUMENTS =====

      -- 19. RECEIPTS (Subscription rights)
      WHEN "MktNm" = 'EQUITY-CASH'
        AND "SctyCtgyNm" = 'RECEIPTS'
      THEN COALESCE("CrpnNm", "AsstDesc") || ' Recibo ' || "TckrSymb"

      -- 20. RIGHTS (Subscription rights)
      WHEN "MktNm" = 'EQUITY-CASH'
        AND "SctyCtgyNm" = 'RIGHTS'
      THEN COALESCE("CrpnNm", "AsstDesc") || ' Direito ' || "TckrSymb"

      -- 21. WARRANTS
      WHEN "MktNm" = 'EQUITY-CASH'
        AND "SctyCtgyNm" = 'WARRANT'
      THEN COALESCE("CrpnNm", "AsstDesc") || ' Warrant ' || "TckrSymb"

      -- 22. INDEX (Market indexes)
      WHEN "MktNm" = 'EQUITY-CASH'
        AND "SctyCtgyNm" = 'INDEX'
      THEN 'Índice ' || COALESCE("AsstDesc", "CrpnNm", "Asst")

      -- 23. EDS (Structured Derivatives)
      WHEN "SctyCtgyNm" IN ('EDS DOL DDI', 'EDS DV01', 'EDS PU NTR')
      THEN 'EDS ' || COALESCE("AsstDesc", "Asst") || ' ' ||
           COALESCE(expiration_date_formatted, '')

      -- 24. FORWARD RATE AGREEMENT
      WHEN "SctyCtgyNm" = 'FORWARD RATE AGREEMENT'
      THEN 'FRA ' || COALESCE("AsstDesc", "Asst") || ' ' ||
           COALESCE(expiration_date_formatted, '')

      -- 25. FORWARD POINTS
      WHEN "SctyCtgyNm" = 'FORWARD POINTS'
      THEN 'Pontos a Termo ' || COALESCE("AsstDesc", "Asst") || ' ' ||
           COALESCE(expiration_date_formatted, '')

      -- 26. Equity Block Trading Lot (same as underlying)
      WHEN "SgmtNm" = 'Equity Block Trading Lot'
        AND "SctyCtgyNm" = 'SHARES'
      THEN COALESCE("CrpnNm", "AsstDesc", "Asst") || ' ' ||
           COALESCE("SpcfctnCd", '') || ' ' || "TckrSymb"

      WHEN "SgmtNm" = 'Equity Block Trading Lot'
        AND "SctyCtgyNm" = 'BDR'
      THEN COALESCE("CrpnNm", "AsstDesc", "Asst") || ' BDR ' ||
           COALESCE("SpcfctnCd", '')


      -- ===== FALLBACK =====

      -- Unknown/unmapped instrument types
      ELSE 'Unknown Instrument Type: ' || "TckrSymb" ||
           COALESCE(' (' || "SctyCtgyNm" || ')', '')

    END AS instrument_name,

    -- Security Type ID mapping based on b3_type_mapping.md
    CASE
      -- ===== FUTURES (SecurityType ID: 10) =====
      WHEN "MktNm" = 'FUTURE' THEN 10

      -- ===== OPTIONS ON EQUITIES (SecurityType ID: 11) =====
      WHEN "MktNm" = 'EQUITY-DERIVATE'
        AND "SctyCtgyNm" = 'OPTION ON EQUITIES' THEN 11

      -- ===== OPTIONS ON FUTURES/INDEX (SecurityType ID: 12) =====
      WHEN "MktNm" = 'EQUITY-DERIVATE'
        AND "SctyCtgyNm" = 'OPTION ON INDEX' THEN 12
      WHEN "MktNm" = 'OPTIONS ON FUTURE' THEN 12
      WHEN "MktNm" = 'OPTIONS ON SPOT' THEN 12

      -- ===== FORWARDS (SecurityType ID: 13) =====
      WHEN "MktNm" = 'EQUITY-DERIVATE'
        AND "SctyCtgyNm" = 'COMMON EQUITIES FORWARD' THEN 13
      WHEN "MktNm" = 'FORWARD' AND "SctyCtgyNm" = 'FX SWAP' THEN 13
      WHEN "SctyCtgyNm" = 'FORWARD RATE AGREEMENT' THEN 13
      WHEN "SctyCtgyNm" = 'FORWARD POINTS' THEN 13

      -- ===== EQUITIES - Shares and Units (SecurityType ID: 1) =====
      WHEN "MktNm" = 'EQUITY-CASH'
        AND "SctyCtgyNm" IN ('SHARES', 'UNIT') THEN 1
      WHEN "SgmtNm" = 'Equity Block Trading Lot'
        AND "SctyCtgyNm" IN ('SHARES', 'UNIT') THEN 1

      -- ===== BDRs (SecurityType ID: 15 - Receipt) =====
      WHEN "MktNm" = 'EQUITY-CASH' AND "SctyCtgyNm" = 'BDR' THEN 15
      WHEN "SgmtNm" = 'Equity Block Trading Lot' AND "SctyCtgyNm" = 'BDR' THEN 15

      -- ===== ETFs (SecurityType ID: 6) =====
      WHEN "MktNm" = 'EQUITY-CASH'
        AND "SctyCtgyNm" IN ('ETF EQUITIES', 'ETF FOREIGN INDEX') THEN 6

      -- ===== FIIs/REITs (SecurityType ID: 7) =====
      -- Note: FIIs appear as FUNDS - identify by corporation name
      WHEN "MktNm" = 'EQUITY-CASH'
        AND "SctyCtgyNm" = 'FUNDS'
        AND ("CrpnNm" ILIKE '%IMOB%' OR "CrpnNm" ILIKE '%FII%') THEN 7

      -- ===== FUNDS (SecurityType ID: 14) =====
      WHEN "MktNm" = 'EQUITY-CASH' AND "SctyCtgyNm" = 'FUNDS' THEN 14

      -- ===== FIXED INCOME - Bonds (SecurityType ID: 2) =====
      WHEN "MktNm" = 'FIXED INCOME' THEN 2
      WHEN "SctyCtgyNm" LIKE '%FIXED INCOME%' THEN 2

      -- ===== RECEIPTS (SecurityType ID: 15) =====
      WHEN "MktNm" = 'EQUITY-CASH' AND "SctyCtgyNm" = 'RECEIPTS' THEN 15

      -- ===== RIGHTS (SecurityType ID: 15 - Receipt) =====
      -- Rights are subscription rights - mapping to Receipt
      WHEN "MktNm" = 'EQUITY-CASH' AND "SctyCtgyNm" = 'RIGHTS' THEN 15

      -- ===== WARRANTS (SecurityType ID: 11 - OptionOnEquity) =====
      -- Warrants are options - mapping to OptionOnEquity
      WHEN "MktNm" = 'EQUITY-CASH' AND "SctyCtgyNm" = 'WARRANT' THEN 11

      -- ===== INDEXES (SecurityType ID: 18) =====
      WHEN "MktNm" = 'EQUITY-CASH' AND "SctyCtgyNm" = 'INDEX' THEN 18

      -- ===== DERIVATIVES - Generic (SecurityType ID: 8) =====
      -- EDS instruments are exotic derivatives/swaps
      WHEN "SctyCtgyNm" IN ('EDS DOL DDI', 'EDS DV01', 'EDS PU NTR') THEN 8

      -- ===== FALLBACK - NULL for unmapped types =====
      ELSE NULL
    END AS securityTypeId

  FROM staging
)

SELECT
  -- Keep all 52 original columns
  "RptDt",
  "TckrSymb",
  "Asst",
  "AsstDesc",
  "SgmtNm",
  "MktNm",
  "SctyCtgyNm",
  "XprtnDt",
  "XprtnCd",
  "TradgStartDt",
  "TradgEndDt",
  "BaseCd",
  "ConvsCritNm",
  "MtrtyDtTrgtPt",
  "ReqrdConvsInd",
  "ISIN",
  "CFICd",
  "DlvryNtceStartDt",
  "DlvryNtceEndDt",
  "OptnTp",
  "CtrctMltplr",
  "AsstQtnQty",
  "AllcnRndLot",
  "TradgCcy",
  "DlvryTpNm",
  "WdrwlDays",
  "WrkgDays",
  "ClnrDays",
  "RlvrBasePricNm",
  "OpngFutrPosDay",
  "SdTpCd1",
  "UndrlygTckrSymb1",
  "SdTpCd2",
  "UndrlygTckrSymb2",
  "PureGoldWght",
  "ExrcPric",
  "OptnStyle",
  "ValTpNm",
  "PrmUpfrntInd",
  "OpngPosLmtDt",
  "DstrbtnId",
  "PricFctr",
  "DaysToSttlm",
  "SrsTpNm",
  "PrtcnFlg",
  "AutomtcExrcInd",
  "SpcfctnCd",
  "CrpnNm",
  "CorpActnStartDt",
  "CtdyTrtmntTpNm",
  "MktCptlstn",
  "CorpGovnLvlNm",

  -- Add the new columns (53rd and 54th columns)
  instrument_name,
  securityTypeId

FROM instrument_names
