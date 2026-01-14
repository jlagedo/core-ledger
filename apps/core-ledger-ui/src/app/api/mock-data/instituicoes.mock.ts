import { TipoVinculo } from '../../features/cadastro/fundos/wizard/models/vinculos.model';

/**
 * Interface de Instituição para mock
 */
export interface MockInstituicao {
  id: string;
  cnpj: string;
  razaoSocial: string;
  nomeFantasia?: string;
  codigoCvm?: string;
  tiposHabilitados: TipoVinculo[];
  ativo: boolean;
}

/**
 * Mock data for instituicoes (financial institutions).
 * Includes major Brazilian fund administrators, custodians, and managers.
 * @internal
 */
export const MOCK_INSTITUICOES: MockInstituicao[] = [
  // Administradores Fiduciários
  {
    id: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    cnpj: '59281253000123',
    razaoSocial: 'BTG PACTUAL SERVIÇOS FINANCEIROS S.A. DTVM',
    nomeFantasia: 'BTG PACTUAL DTVM',
    codigoCvm: '20011',
    tiposHabilitados: [TipoVinculo.ADMINISTRADOR, TipoVinculo.CUSTODIANTE, TipoVinculo.DISTRIBUIDOR],
    ativo: true,
  },
  {
    id: 'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    cnpj: '33042953000104',
    razaoSocial: 'INTRAG DISTRIBUIDORA DE TITULOS E VALORES MOBILIARIOS LTDA',
    nomeFantasia: 'INTRAG DTVM',
    codigoCvm: '00011',
    tiposHabilitados: [TipoVinculo.ADMINISTRADOR, TipoVinculo.CUSTODIANTE],
    ativo: true,
  },
  {
    id: 'c3d4e5f6-a7b8-9012-cdef-123456789012',
    cnpj: '62232889000190',
    razaoSocial: 'BNY MELLON SERVICOS FINANCEIROS DTVM S.A.',
    nomeFantasia: 'BNY MELLON',
    codigoCvm: '02011',
    tiposHabilitados: [TipoVinculo.ADMINISTRADOR, TipoVinculo.CUSTODIANTE],
    ativo: true,
  },
  {
    id: 'd4e5f6a7-b8c9-0123-defa-234567890123',
    cnpj: '02658435000115',
    razaoSocial: 'SINGULARE CORRETORA DE TÍTULOS E VALORES MOBILIÁRIOS S.A.',
    nomeFantasia: 'SINGULARE',
    codigoCvm: '16011',
    tiposHabilitados: [TipoVinculo.ADMINISTRADOR, TipoVinculo.CUSTODIANTE, TipoVinculo.ESCRITURADOR],
    ativo: true,
  },

  // Gestores de Recursos
  {
    id: 'e5f6a7b8-c9d0-1234-efab-345678901234',
    cnpj: '08604465000133',
    razaoSocial: 'KINEA INVESTIMENTOS LTDA',
    nomeFantasia: 'KINEA',
    codigoCvm: '15011',
    tiposHabilitados: [TipoVinculo.GESTOR],
    ativo: true,
  },
  {
    id: 'f6a7b8c9-d0e1-2345-fabc-456789012345',
    cnpj: '07442731000155',
    razaoSocial: 'XP GESTÃO DE RECURSOS LTDA',
    nomeFantasia: 'XP GESTÃO',
    codigoCvm: '14011',
    tiposHabilitados: [TipoVinculo.GESTOR],
    ativo: true,
  },
  {
    id: 'a7b8c9d0-e1f2-3456-abcd-567890123456',
    cnpj: '10690848000118',
    razaoSocial: 'VINCI PARTNERS INVESTIMENTOS LTDA',
    nomeFantasia: 'VINCI PARTNERS',
    codigoCvm: '17011',
    tiposHabilitados: [TipoVinculo.GESTOR],
    ativo: true,
  },
  {
    id: 'b8c9d0e1-f2a3-4567-bcde-678901234567',
    cnpj: '18237839000103',
    razaoSocial: 'KAPITALO INVESTIMENTOS LTDA',
    nomeFantasia: 'KAPITALO',
    codigoCvm: '18011',
    tiposHabilitados: [TipoVinculo.GESTOR],
    ativo: true,
  },

  // Custodiantes
  {
    id: 'c9d0e1f2-a3b4-5678-cdef-789012345678',
    cnpj: '00000000000191',
    razaoSocial: 'BANCO DO BRASIL S.A.',
    nomeFantasia: 'BANCO DO BRASIL',
    codigoCvm: '00001',
    tiposHabilitados: [TipoVinculo.CUSTODIANTE, TipoVinculo.DISTRIBUIDOR],
    ativo: true,
  },
  {
    id: 'd0e1f2a3-b4c5-6789-defa-890123456789',
    cnpj: '60746948000112',
    razaoSocial: 'BANCO BRADESCO S.A.',
    nomeFantasia: 'BRADESCO',
    codigoCvm: '00002',
    tiposHabilitados: [TipoVinculo.CUSTODIANTE, TipoVinculo.DISTRIBUIDOR, TipoVinculo.ADMINISTRADOR],
    ativo: true,
  },
  {
    id: 'e1f2a3b4-c5d6-7890-efab-901234567890',
    cnpj: '90400888000142',
    razaoSocial: 'BANCO SANTANDER (BRASIL) S.A.',
    nomeFantasia: 'SANTANDER',
    codigoCvm: '00003',
    tiposHabilitados: [TipoVinculo.CUSTODIANTE, TipoVinculo.DISTRIBUIDOR],
    ativo: true,
  },

  // Auditores
  {
    id: 'f2a3b4c5-d6e7-8901-fabc-012345678901',
    cnpj: '61562112000120',
    razaoSocial: 'DELOITTE TOUCHE TOHMATSU AUDITORES INDEPENDENTES',
    nomeFantasia: 'DELOITTE',
    tiposHabilitados: [TipoVinculo.AUDITOR],
    ativo: true,
  },
  {
    id: 'a3b4c5d6-e7f8-9012-abcd-123456789012',
    cnpj: '57755217000129',
    razaoSocial: 'KPMG AUDITORES INDEPENDENTES',
    nomeFantasia: 'KPMG',
    tiposHabilitados: [TipoVinculo.AUDITOR],
    ativo: true,
  },
  {
    id: 'b4c5d6e7-f8a9-0123-bcde-234567890123',
    cnpj: '61366936000125',
    razaoSocial: 'PRICEWATERHOUSECOOPERS AUDITORES INDEPENDENTES',
    nomeFantasia: 'PwC',
    tiposHabilitados: [TipoVinculo.AUDITOR],
    ativo: true,
  },
  {
    id: 'c5d6e7f8-a9b0-1234-cdef-345678901234',
    cnpj: '67945071000138',
    razaoSocial: 'ERNST & YOUNG AUDITORES INDEPENDENTES S.S.',
    nomeFantasia: 'EY',
    tiposHabilitados: [TipoVinculo.AUDITOR],
    ativo: true,
  },

  // Distribuidores
  {
    id: 'd6e7f8a9-b0c1-2345-defa-456789012345',
    cnpj: '02332886000104',
    razaoSocial: 'XP INVESTIMENTOS CORRETORA DE CÂMBIO, TÍTULOS E VALORES MOBILIÁRIOS S.A.',
    nomeFantasia: 'XP INVESTIMENTOS',
    codigoCvm: '19011',
    tiposHabilitados: [TipoVinculo.DISTRIBUIDOR],
    ativo: true,
  },
  {
    id: 'e7f8a9b0-c1d2-3456-efab-567890123456',
    cnpj: '18945670000146',
    razaoSocial: 'NU INVEST CORRETORA DE VALORES S.A.',
    nomeFantasia: 'NUINVEST',
    codigoCvm: '21011',
    tiposHabilitados: [TipoVinculo.DISTRIBUIDOR],
    ativo: true,
  },

  // FIDC Específicos
  {
    id: 'f8a9b0c1-d2e3-4567-fabc-678901234567',
    cnpj: '04902979000144',
    razaoSocial: 'TERCON CONSULTORIA EMPRESARIAL LTDA',
    nomeFantasia: 'TERCON',
    tiposHabilitados: [TipoVinculo.CONSULTORIA_CREDITO, TipoVinculo.AGENTE_COBRANCA],
    ativo: true,
  },
  {
    id: 'a9b0c1d2-e3f4-5678-abcd-789012345678',
    cnpj: '09626302000160',
    razaoSocial: 'SOLIS INVESTIMENTOS LTDA',
    nomeFantasia: 'SOLIS',
    tiposHabilitados: [TipoVinculo.CONSULTORIA_CREDITO, TipoVinculo.GESTOR],
    ativo: true,
  },
  {
    id: 'b0c1d2e3-f4a5-6789-bcde-890123456789',
    cnpj: '10835932000108',
    razaoSocial: 'MULTIPLIKE ASSESSORIA E CONSULTORIA EMPRESARIAL LTDA',
    nomeFantasia: 'MULTIPLIKE',
    tiposHabilitados: [TipoVinculo.AGENTE_COBRANCA],
    ativo: true,
  },

  // Escrituradores
  {
    id: 'c1d2e3f4-a5b6-7890-cdef-901234567890',
    cnpj: '29030467000101',
    razaoSocial: 'VÓRTX DISTRIBUIDORA DE TÍTULOS E VALORES MOBILIÁRIOS LTDA',
    nomeFantasia: 'VÓRTX',
    codigoCvm: '22011',
    tiposHabilitados: [TipoVinculo.ESCRITURADOR, TipoVinculo.ADMINISTRADOR],
    ativo: true,
  },
];
