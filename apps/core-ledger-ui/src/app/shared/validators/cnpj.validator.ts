import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * CNPJ Validator - Validates CNPJ format and check digits using Receita Federal algorithm.
 *
 * CNPJ structure: XX.XXX.XXX/XXXX-XX (14 characters)
 * - First 12 characters: Registration number (can include letters A-Z and digits)
 * - Last 2 digits: Check digits (mod 11 algorithm)
 *
 * Based on official Receita Federal validation rules.
 */
export function cnpjValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;

    if (!value) {
      return null; // Let required validator handle empty values
    }

    if (!isValidCNPJ(value)) {
      return { cnpjInvalid: { message: 'CNPJ invalido' } };
    }

    return null;
  };
}

/**
 * Validates if CNPJ is valid according to Receita Federal rules
 */
function isValidCNPJ(cnpj: string): boolean {
  const REGEX_CARACTERES_NAO_PERMITIDOS = /[^A-Z\d./-]/i;
  const REGEX_CNPJ = /^([A-Z\d]){12}(\d){2}$/;
  const CNPJ_ZERADO = '00000000000000';

  // Check for invalid characters
  if (REGEX_CARACTERES_NAO_PERMITIDOS.test(cnpj)) {
    return false;
  }

  // Remove mask
  const cnpjSemMascara = removeMascaraCNPJ(cnpj);

  // Check format and zero CNPJ
  if (!REGEX_CNPJ.test(cnpjSemMascara) || cnpjSemMascara === CNPJ_ZERADO) {
    return false;
  }

  // Validate check digits
  const dvInformado = cnpjSemMascara.substring(12);
  const dvCalculado = calculaDV(cnpjSemMascara.substring(0, 12));

  return dvInformado === dvCalculado;
}

/**
 * Calculates CNPJ check digits (DV)
 * @param cnpj - CNPJ without check digits (12 characters)
 * @returns Check digits as string (2 digits)
 */
function calculaDV(cnpj: string): string {
  const REGEX_CARACTERES_NAO_PERMITIDOS = /[^A-Z\d./-]/i;
  const REGEX_CNPJ_SEM_DV = /^([A-Z\d]){12}$/;
  const PESOS_DV = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
  const VALOR_BASE = '0'.charCodeAt(0);
  const TAMANHO_CNPJ_SEM_DV = 12;

  // Check for invalid characters
  if (REGEX_CARACTERES_NAO_PERMITIDOS.test(cnpj)) {
    throw new Error('CNPJ contem caracteres invalidos');
  }

  // Remove mask
  const cnpjSemMascara = removeMascaraCNPJ(cnpj);

  // Validate format
  if (!REGEX_CNPJ_SEM_DV.test(cnpjSemMascara)) {
    throw new Error('CNPJ sem DV deve ter 12 caracteres alfanumericos');
  }

  let somatorioDV1 = 0;
  let somatorioDV2 = 0;

  for (let i = 0; i < TAMANHO_CNPJ_SEM_DV; i++) {
    const asciiDigito = cnpjSemMascara.charCodeAt(i) - VALOR_BASE;
    somatorioDV1 += asciiDigito * PESOS_DV[i + 1];
    somatorioDV2 += asciiDigito * PESOS_DV[i];
  }

  const dv1 = somatorioDV1 % 11 < 2 ? 0 : 11 - (somatorioDV1 % 11);
  somatorioDV2 += dv1 * PESOS_DV[TAMANHO_CNPJ_SEM_DV];
  const dv2 = somatorioDV2 % 11 < 2 ? 0 : 11 - (somatorioDV2 % 11);

  return `${dv1}${dv2}`;
}

/**
 * Removes CNPJ mask characters (. / -)
 */
function removeMascaraCNPJ(cnpj: string): string {
  const REGEX_CARACTERES_MASCARA = /[./-]/g;
  return cnpj.replace(REGEX_CARACTERES_MASCARA, '');
}
