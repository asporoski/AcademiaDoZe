-- Amanda Soares Poroski

-- ============================================================
-- SCRIPT DE CRIAÇÃO DO BANCO DE DADOS: db_academia_do_ze
-- SQL Server
-- Baseado na camada de domínio AcademiaDoZe.Domain
-- ============================================================

IF OBJECT_ID(N'dbo.tb_logradouro', N'U') IS NULL
BEGIN
    CREATE TABLE tb_logradouro (
        id_logradouro INT IDENTITY(1,1) NOT NULL,
        cep VARCHAR(8) NOT NULL,
        nome VARCHAR(150) NOT NULL,
        bairro VARCHAR(100) NOT NULL,
        cidade VARCHAR(100) NOT NULL,
        estado CHAR(2) NOT NULL,
        pais VARCHAR(50) NOT NULL
            CONSTRAINT DF_tb_logradouro_pais DEFAULT 'Brasil',

        CONSTRAINT PK_tb_logradouro
            PRIMARY KEY (id_logradouro),

        CONSTRAINT UQ_tb_logradouro_cep
            UNIQUE (cep)
    );

    CREATE INDEX IX_tb_logradouro_cep
        ON tb_logradouro(cep);

    CREATE INDEX IX_tb_logradouro_cidade
        ON tb_logradouro(cidade);
END;