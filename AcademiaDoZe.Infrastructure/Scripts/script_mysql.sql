-- Amanda Soares Poroski

-- ============================================================
-- SCRIPT DE CRIAÇÃO DO BANCO DE DADOS: db_academia_do_ze
-- MySQL / MariaDB
-- Baseado na camada de domínio AcademiaDoZe.Domain
-- ============================================================

CREATE TABLE IF NOT EXISTS tb_logradouro (
    id_logradouro INT AUTO_INCREMENT NOT NULL,
    cep VARCHAR(8) NOT NULL,
    nome VARCHAR(150) NOT NULL,
    bairro VARCHAR(100) NOT NULL,
    cidade VARCHAR(100) NOT NULL,
    estado CHAR(2) NOT NULL,
    pais VARCHAR(50) NOT NULL DEFAULT 'Brasil',

    PRIMARY KEY (id_logradouro),
    UNIQUE KEY uq_tb_logradouro_cep (cep),
    INDEX ix_tb_logradouro_cep (cep),
    INDEX ix_tb_logradouro_cidade (cidade)
) ENGINE=InnoDB;