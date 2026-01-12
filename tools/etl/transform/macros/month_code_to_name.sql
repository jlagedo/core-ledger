{% macro month_code_to_name(month_code_column) %}
  CASE {{ month_code_column }}
    WHEN 'F' THEN 'Janeiro'
    WHEN 'G' THEN 'Fevereiro'
    WHEN 'H' THEN 'Março'
    WHEN 'J' THEN 'Abril'
    WHEN 'K' THEN 'Maio'
    WHEN 'M' THEN 'Junho'
    WHEN 'N' THEN 'Julho'
    WHEN 'Q' THEN 'Agosto'
    WHEN 'U' THEN 'Setembro'
    WHEN 'V' THEN 'Outubro'
    WHEN 'X' THEN 'Novembro'
    WHEN 'Z' THEN 'Dezembro'
    ELSE NULL
  END
{% endmacro %}
