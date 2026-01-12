{{
  config(
    materialized='view',
    description='Staging model with helper columns for B3 instruments'
  )
}}

WITH source AS (
  SELECT * FROM {{ source('b3_raw', 'b3_instruments') }}
),

helpers AS (
  SELECT
    *,

    -- Extract year from expiration code (last 2 digits of XprtnCd)
    CASE
      WHEN "XprtnCd" IS NOT NULL AND LENGTH("XprtnCd") >= 3
      THEN '20' || SUBSTRING("XprtnCd", 2, 2)
      ELSE NULL
    END AS expiration_year,

    -- Convert month code to Portuguese month name
    {{ month_code_to_name('LEFT("XprtnCd", 1)') }} AS expiration_month_name,

    -- Call/Put in Portuguese
    CASE "OptnTp"
      WHEN 'Call' THEN 'Call'
      WHEN 'Put' THEN 'Put'
      ELSE NULL
    END AS option_type_pt,

    -- Format strike price (remove trailing zeros)
    -- Note: Replace comma with period for Brazilian number format
    CASE
      WHEN "ExrcPric" IS NOT NULL AND "ExrcPric" != ''
      THEN TRIM(TO_CHAR(REPLACE("ExrcPric", ',', '.')::NUMERIC, 'FM999999990.00'))
      ELSE NULL
    END AS strike_formatted,

    -- Format expiration date as DD/MM/YYYY
    CASE
      WHEN "XprtnDt" IS NOT NULL AND "XprtnDt" != '9999-12-31'
      THEN TO_CHAR(TO_DATE("XprtnDt", 'YYYY-MM-DD'), 'DD/MM/YYYY')
      ELSE NULL
    END AS expiration_date_formatted,

    -- Identify if ticker suggests mini contract (WDO, WIN, etc.)
    CASE
      WHEN "Asst" IN ('WDO', 'WIN', 'WFR', 'WSP') OR "AsstDesc" ILIKE '%mini%'
      THEN TRUE
      ELSE FALSE
    END AS is_mini_contract

  FROM source
)

SELECT * FROM helpers
