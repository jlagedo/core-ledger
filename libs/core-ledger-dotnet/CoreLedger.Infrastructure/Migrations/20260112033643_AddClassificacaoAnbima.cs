using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClassificacaoAnbima : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "classificacao_anbima",
                schema: "cadastros",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nivel1 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nivel2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nivel3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    classificacao_cvm = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ordem_exibicao = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_classificacao_anbima", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_classificacao_anbima_ativo",
                schema: "cadastros",
                table: "classificacao_anbima",
                column: "ativo",
                filter: "ativo = true");

            migrationBuilder.CreateIndex(
                name: "ix_classificacao_anbima_classificacao_cvm",
                schema: "cadastros",
                table: "classificacao_anbima",
                column: "classificacao_cvm");

            migrationBuilder.CreateIndex(
                name: "ix_classificacao_anbima_codigo",
                schema: "cadastros",
                table: "classificacao_anbima",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_classificacao_anbima_nivel1",
                schema: "cadastros",
                table: "classificacao_anbima",
                column: "nivel1");

            // Seed data: ANBIMA classifications
            migrationBuilder.Sql(@"
-- Renda Fixa (15 classifications)
INSERT INTO cadastros.classificacao_anbima (codigo, nome, nivel1, nivel2, nivel3, classificacao_cvm, ordem_exibicao, descricao, created_at) VALUES
('RF_DB_SOB', 'Renda Fixa Duração Baixa Soberano', 'Renda Fixa', 'Duração Baixa', 'Soberano', 'RENDA_FIXA', 1, 'Fundos que investem em títulos públicos federais com duration média até 21 dias úteis', NOW()),
('RF_DB_GRD', 'Renda Fixa Duração Baixa Grau de Investimento', 'Renda Fixa', 'Duração Baixa', 'Grau de Investimento', 'RENDA_FIXA', 2, 'Fundos com duration baixa e foco em crédito de alta qualidade', NOW()),
('RF_DB_CL', 'Renda Fixa Duração Baixa Crédito Livre', 'Renda Fixa', 'Duração Baixa', 'Crédito Livre', 'RENDA_FIXA', 3, 'Fundos com duration baixa sem restrição de crédito', NOW()),
('RF_DM_SOB', 'Renda Fixa Duração Média Soberano', 'Renda Fixa', 'Duração Média', 'Soberano', 'RENDA_FIXA', 4, 'Fundos soberanos com duration média entre 21 e 126 dias úteis', NOW()),
('RF_DM_GRD', 'Renda Fixa Duração Média Grau de Investimento', 'Renda Fixa', 'Duração Média', 'Grau de Investimento', 'RENDA_FIXA', 5, 'Fundos com duration média e crédito de alta qualidade', NOW()),
('RF_DM_CL', 'Renda Fixa Duração Média Crédito Livre', 'Renda Fixa', 'Duração Média', 'Crédito Livre', 'RENDA_FIXA', 6, 'Fundos com duration média sem restrição de crédito', NOW()),
('RF_DA_SOB', 'Renda Fixa Duração Alta Soberano', 'Renda Fixa', 'Duração Alta', 'Soberano', 'RENDA_FIXA', 7, 'Fundos soberanos com duration acima de 126 dias úteis', NOW()),
('RF_DA_GRD', 'Renda Fixa Duração Alta Grau de Investimento', 'Renda Fixa', 'Duração Alta', 'Grau de Investimento', 'RENDA_FIXA', 8, 'Fundos com duration alta e crédito de alta qualidade', NOW()),
('RF_DA_CL', 'Renda Fixa Duração Alta Crédito Livre', 'Renda Fixa', 'Duração Alta', 'Crédito Livre', 'RENDA_FIXA', 9, 'Fundos com duration alta sem restrição de crédito', NOW()),
('RF_DL_SOB', 'Renda Fixa Duração Livre Soberano', 'Renda Fixa', 'Duração Livre', 'Soberano', 'RENDA_FIXA', 10, 'Fundos soberanos sem compromisso de duration', NOW()),
('RF_DL_GRD', 'Renda Fixa Duração Livre Grau de Investimento', 'Renda Fixa', 'Duração Livre', 'Grau de Investimento', 'RENDA_FIXA', 11, 'Fundos com duration livre e crédito de alta qualidade', NOW()),
('RF_DL_CL', 'Renda Fixa Duração Livre Crédito Livre', 'Renda Fixa', 'Duração Livre', 'Crédito Livre', 'RENDA_FIXA', 12, 'Fundos com duration livre sem restrição de crédito', NOW()),
('RF_IDX', 'Renda Fixa Indexados', 'Renda Fixa', 'Indexados', NULL, 'RENDA_FIXA', 13, 'Fundos que buscam seguir índices de renda fixa como IMA-B', NOW()),
('RF_INV_EXT', 'Renda Fixa Investimento no Exterior', 'Renda Fixa', 'Investimento no Exterior', NULL, 'RENDA_FIXA', 14, 'Fundos de renda fixa com investimentos no exterior', NOW()),
('RF_SIMPLES', 'Renda Fixa Simples', 'Renda Fixa', 'Simples', NULL, 'RENDA_FIXA', 15, 'Fundos destinados a investidores de varejo com política de investimento simplificada', NOW());

-- Ações (13 classifications)
INSERT INTO cadastros.classificacao_anbima (codigo, nome, nivel1, nivel2, nivel3, classificacao_cvm, ordem_exibicao, descricao, created_at) VALUES
('AE_IDX_ATIVO', 'Ações Indexados Índice Ativo', 'Ações', 'Indexados', 'Índice Ativo', 'ACOES', 20, 'Fundos que buscam superar índices de ações', NOW()),
('AE_IDX_IBOV', 'Ações Indexados Ibovespa Ativo', 'Ações', 'Indexados', 'Ibovespa Ativo', 'ACOES', 21, 'Fundos que buscam superar o Ibovespa', NOW()),
('AE_IDX_IBRX', 'Ações Indexados IBrX Ativo', 'Ações', 'Indexados', 'IBrX Ativo', 'ACOES', 22, 'Fundos que buscam superar o IBrX', NOW()),
('AE_ATI_VAL', 'Ações Ativos Valor/Crescimento', 'Ações', 'Ativos', 'Valor/Crescimento', 'ACOES', 23, 'Fundos com estratégia value ou growth', NOW()),
('AE_ATI_SET', 'Ações Ativos Setoriais', 'Ações', 'Ativos', 'Setoriais', 'ACOES', 24, 'Fundos focados em setores específicos', NOW()),
('AE_ATI_DIV', 'Ações Ativos Dividendos', 'Ações', 'Ativos', 'Dividendos', 'ACOES', 25, 'Fundos focados em empresas pagadoras de dividendos', NOW()),
('AE_ATI_SC', 'Ações Ativos Small Caps', 'Ações', 'Ativos', 'Small Caps', 'ACOES', 26, 'Fundos focados em empresas de menor capitalização', NOW()),
('AE_ATI_SUS', 'Ações Ativos Sustentabilidade/Governança', 'Ações', 'Ativos', 'Sustentabilidade/Governança', 'ACOES', 27, 'Fundos com critérios ESG', NOW()),
('AE_ATI_LIV', 'Ações Ativos Livre', 'Ações', 'Ativos', 'Livre', 'ACOES', 28, 'Fundos de ações sem estratégia específica', NOW()),
('AE_ESP_FMP', 'Ações Específicos FMP-FGTS', 'Ações', 'Específicos', 'FMP-FGTS', 'ACOES', 29, 'Fundos de privatização com recursos do FGTS', NOW()),
('AE_ESP_FEC', 'Ações Específicos Fechados de Ações', 'Ações', 'Específicos', 'Fechados de Ações', 'ACOES', 30, 'Fundos fechados de ações', NOW()),
('AE_ESP_MON', 'Ações Específicos Mono Ação', 'Ações', 'Específicos', 'Mono Ação', 'ACOES', 31, 'Fundos que investem em uma única ação', NOW()),
('AE_INV_EXT', 'Ações Investimento no Exterior', 'Ações', 'Investimento no Exterior', NULL, 'ACOES', 32, 'Fundos de ações com investimentos no exterior', NOW());

-- Multimercado (11 classifications)
INSERT INTO cadastros.classificacao_anbima (codigo, nome, nivel1, nivel2, nivel3, classificacao_cvm, ordem_exibicao, descricao, created_at) VALUES
('MM_MACRO', 'Multimercado Macro', 'Multimercado', 'Alocação', 'Macro', 'MULTIMERCADO', 40, 'Fundos com estratégia baseada em cenários macroeconômicos', NOW()),
('MM_DINAMIC', 'Multimercado Dinâmico', 'Multimercado', 'Alocação', 'Dinâmico', 'MULTIMERCADO', 41, 'Fundos com alocação dinâmica entre classes de ativos', NOW()),
('MM_TRADING', 'Multimercado Trading', 'Multimercado', 'Estratégia', 'Trading', 'MULTIMERCADO', 42, 'Fundos com estratégia de curto prazo', NOW()),
('MM_LS_DIR', 'Multimercado Long and Short Direcional', 'Multimercado', 'Estratégia', 'Long and Short Direcional', 'MULTIMERCADO', 43, 'Fundos long/short com exposição direcional', NOW()),
('MM_LS_NEU', 'Multimercado Long and Short Neutro', 'Multimercado', 'Estratégia', 'Long and Short Neutro', 'MULTIMERCADO', 44, 'Fundos long/short com exposição neutra', NOW()),
('MM_JRS', 'Multimercado Juros e Moedas', 'Multimercado', 'Estratégia', 'Juros e Moedas', 'MULTIMERCADO', 45, 'Fundos focados em juros e câmbio', NOW()),
('MM_LIVRE', 'Multimercado Livre', 'Multimercado', 'Estratégia', 'Livre', 'MULTIMERCADO', 46, 'Fundos sem estratégia específica definida', NOW()),
('MM_CAP_PROT', 'Multimercado Capital Protegido', 'Multimercado', 'Estratégia', 'Capital Protegido', 'MULTIMERCADO', 47, 'Fundos com proteção de capital', NOW()),
('MM_EST_ESP', 'Multimercado Estratégia Específica', 'Multimercado', 'Estratégia', 'Específica', 'MULTIMERCADO', 48, 'Fundos com estratégias específicas não enquadradas', NOW()),
('MM_BAL', 'Multimercado Balanceados', 'Multimercado', 'Alocação', 'Balanceados', 'MULTIMERCADO', 49, 'Fundos com alocação balanceada entre classes', NOW()),
('MM_INV_EXT', 'Multimercado Investimento no Exterior', 'Multimercado', 'Investimento no Exterior', NULL, 'MULTIMERCADO', 50, 'Fundos multimercado com investimentos no exterior', NOW());

-- Cambial (3 classifications)
INSERT INTO cadastros.classificacao_anbima (codigo, nome, nivel1, nivel2, nivel3, classificacao_cvm, ordem_exibicao, descricao, created_at) VALUES
('CB_DOLAR', 'Cambial Dólar', 'Cambial', 'Moeda', 'Dólar', 'CAMBIAL', 60, 'Fundos com exposição ao dólar americano', NOW()),
('CB_EURO', 'Cambial Euro', 'Cambial', 'Moeda', 'Euro', 'CAMBIAL', 61, 'Fundos com exposição ao euro', NOW()),
('CB_OUTRAS', 'Cambial Outras Moedas', 'Cambial', 'Moeda', 'Outras', 'CAMBIAL', 62, 'Fundos com exposição a outras moedas', NOW());
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "classificacao_anbima",
                schema: "cadastros");
        }
    }
}
