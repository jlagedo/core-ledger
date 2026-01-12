#!/bin/bash

# Angular 21 Compliance Quick Check Script
# Usage: ./scripts/check-compliance.sh

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}   Angular 21 Compliance Quick Check${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""

ISSUES=0

# Function to check pattern
check_pattern() {
  local name=$1
  local pattern=$2
  local file_type=$3
  local description=$4

  echo -n "Checking $name... "

  COUNT=$(grep -r "$pattern" src/app --include="*.$file_type" 2>/dev/null | wc -l | tr -d ' ')

  if [ "$COUNT" -eq 0 ]; then
    echo -e "${GREEN}✓ Pass${NC}"
  else
    echo -e "${RED}✗ Found $COUNT occurrences${NC}"
    echo -e "  ${YELLOW}Issue: $description${NC}"
    ISSUES=$((ISSUES + 1))

    # Show first few occurrences
    if [ "$COUNT" -lt 5 ]; then
      grep -rn "$pattern" src/app --include="*.$file_type" 2>/dev/null | head -3 | while read line; do
        echo -e "  ${RED}→${NC} $line"
      done
    else
      echo -e "  ${YELLOW}→ Run: grep -rn '$pattern' src/app --include='*.$file_type'${NC}"
    fi
    echo ""
  fi
}

# Route Parameters
check_pattern \
  "route.snapshot" \
  "route\.snapshot" \
  "ts" \
  "Use toSignal(route.paramMap) instead"

# Old Control Flow
check_pattern \
  "*ngIf/*ngFor in templates" \
  "\*ngIf\|\*ngFor\|\*ngSwitch" \
  "html" \
  "Use @if, @for, @switch instead"

# Old Decorators
check_pattern \
  "@Input/@Output decorators" \
  "@Input()\|@Output()" \
  "ts" \
  "Use input() and output() functions instead"

check_pattern \
  "@ViewChild decorators" \
  "@ViewChild\|@ViewChildren\|@ContentChild" \
  "ts" \
  "Use viewChild() and contentChild() functions instead"

check_pattern \
  "@HostBinding/@HostListener" \
  "@HostBinding\|@HostListener" \
  "ts" \
  "Use host object in @Component/@Directive instead"

# NgClass/NgStyle
check_pattern \
  "[ngClass]/[ngStyle] bindings" \
  "\[ngClass\]\|\[ngStyle\]" \
  "html" \
  "Use [class.xxx] and [style.xxx] bindings instead"

# NgModules
check_pattern \
  "@NgModule declarations" \
  "@NgModule" \
  "ts" \
  "Use standalone components instead"

# Constructor injection
echo -n "Checking constructor injection... "
CONSTRUCTOR_COUNT=$(grep -r "constructor(" src/app --include="*.ts" 2>/dev/null | grep -v "constructor()" | grep -v ".spec.ts" | wc -l | tr -d ' ')

if [ "$CONSTRUCTOR_COUNT" -eq 0 ]; then
  echo -e "${GREEN}✓ Pass${NC}"
else
  echo -e "${YELLOW}⚠ Found $CONSTRUCTOR_COUNT occurrences${NC}"
  echo -e "  ${YELLOW}Review: Prefer inject() function at class level${NC}"
  echo ""
fi

echo ""
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"

if [ $ISSUES -eq 0 ]; then
  echo -e "${GREEN}✓ All checks passed! Project is 100% compliant.${NC}"
  exit 0
else
  echo -e "${RED}✗ Found $ISSUES issue(s). Please review and fix.${NC}"
  echo ""
  echo "For detailed guidance, see:"
  echo "  • ANGULAR_COMPLIANCE_REVIEW.md - Full audit process"
  echo "  • .github/COMPLIANCE_QUICK_CHECK.md - Quick reference"
  exit 1
fi
