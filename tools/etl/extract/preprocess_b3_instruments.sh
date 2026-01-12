#!/bin/bash
#
# Preprocessor script for B3 Instruments CSV file
# - Removes the first metadata line ("Status do Arquivo: Final")
# - Converts encoding from Latin-1 (ISO-8859-1) to UTF-8
#

set -e

INPUT_FILE="${1:-InstrumentsConsolidatedFile_20251226_1.csv}"
OUTPUT_FILE="${2:-InstrumentsConsolidatedFile_clean.csv}"

echo "Preprocessing B3 Instruments file..."
echo "Input:  $INPUT_FILE"
echo "Output: $OUTPUT_FILE"

# Remove first line and convert from Latin-1 to UTF-8
tail -n +2 "$INPUT_FILE" | iconv -f ISO-8859-1 -t UTF-8 > "$OUTPUT_FILE"

echo "Preprocessing complete!"
echo "Lines processed: $(wc -l < "$OUTPUT_FILE")"
