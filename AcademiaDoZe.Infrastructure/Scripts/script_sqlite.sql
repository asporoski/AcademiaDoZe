-- Amanda Soares Poroski

-- ============================================================
-- SCRIPT DE CRIAÇÃO DO BANCO DE DADOS: db_academia_do_ze
-- SQLite
-- Baseado na camada de domínio AcademiaDoZe.Domain
-- ============================================================

PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS tb_logradouro (
    id_logradouro INTEGER PRIMARY KEY AUTOINCREMENT,
    cep TEXT NOT NULL UNIQUE,
    nome TEXT NOT NULL,
    bairro TEXT NOT NULL,
    cidade TEXT NOT NULL,
    estado TEXT NOT NULL,
    pais TEXT NOT NULL DEFAULT 'Brasil'
);

CREATE INDEX IF NOT EXISTS ix_tb_logradouro_cep
ON tb_logradouro(cep);

CREATE INDEX IF NOT EXISTS ix_tb_logradouro_cidade
ON tb_logradouro(cidade);