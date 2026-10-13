-- =============================================================================
-- SCRIPT DE POVOAMENTO DO BANCO DE DADOS (SEED DATA)
-- Sistema: Vet Control - Controle de Vacinas para Animais de Grande Porte
-- 
-- Conteúdo gerado:
--  - 5 Espécies de Grande Porte (PortePadrao = 2)
--  - 2 Raças para cada Espécie (Total: 10 Raças)
--  - 5 Animais para cada Raça (Total: 50 Animais)
--  - DataNascimentoAproximada preenchida para animais com data estimada
--  - 2 Identificadores por Animal (Principal + Secundário: SISBOV, Brinco, Microchip, Registro, Tatuagem)
--  - 20 Vacinas Imunobiológicas
--  - 3 Protocolos Vacinais e 7 doses de protocolo
--  - 6 Atribuições de protocolos a animais
--  - Histórico inicial de movimentação para todo o plantel
--  - Aplicações de vacina avulsas e vinculadas a doses de protocolos
--
-- Padrão de GUIDs Incrementais:
--  - Espécies:        00000001-0000-0000-0000-000000000001 .. 00000001-0000-0000-0000-000000000005
--  - Raças:           00000002-0000-0000-0000-000000000001 .. 00000002-0000-0000-0000-000000000010
--  - Animais:         00000003-0000-0000-0000-000000000001 .. 00000003-0000-0000-0000-000000000050
--  - Vacinas:         00000004-0000-0000-0000-000000000001 .. 00000004-0000-0000-0000-000000000020
--  - Identificadores: 00000005-0000-0000-0000-000000000001 .. 00000005-0000-0000-0000-000000000100
--  - Protocolos:      00000007-0000-0000-0000-000000000001 .. 00000007-0000-0000-0000-000000000003
--  - Doses:           00000008-0000-0000-0000-000000000001 .. 00000008-0000-0000-0000-000000000007
--  - Atribuições:     00000009-0000-0000-0000-000000000001 .. 00000009-0000-0000-0000-000000000006
-- =============================================================================

START TRANSACTION;

-- -----------------------------------------------------------------------------
-- 1. ESPÉCIES (5 Espécies de Grande Porte - PortePadrao = 2)
-- -----------------------------------------------------------------------------
INSERT INTO "Especies" ("Id", "Nome", "NomeCientifico", "PortePadrao", "IconeKey", "CreationDateTime", "LastModificationDateTime")
VALUES
    ('00000001-0000-0000-0000-000000000001', 'Bovino', 'Bos taurus', 2, 'bovino', NOW(), NULL),
    ('00000001-0000-0000-0000-000000000002', 'Equino', 'Equus caballus', 2, 'equino', NOW(), NULL),
    ('00000001-0000-0000-0000-000000000003', 'Bubalino', 'Bubalus bubalis', 2, 'bovino', NOW(), NULL),
    ('00000001-0000-0000-0000-000000000004', 'Asinino', 'Equus asinus', 2, 'equino', NOW(), NULL),
    ('00000001-0000-0000-0000-000000000005', 'Muar', 'Equus caballus x Equus asinus', 2, 'equino', NOW(), NULL);

-- -----------------------------------------------------------------------------
-- 2. RAÇAS (2 Raças por Espécie = 10 Raças)
-- -----------------------------------------------------------------------------
INSERT INTO "Racas" ("Id", "EspecieId", "Nome", "CreationDateTime", "LastModificationDateTime")
VALUES
    -- Raças de Bovinos
    ('00000002-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000001', 'Nelore', NOW(), NULL),
    ('00000002-0000-0000-0000-000000000002', '00000001-0000-0000-0000-000000000001', 'Holandês', NOW(), NULL),

    -- Raças de Equinos
    ('00000002-0000-0000-0000-000000000003', '00000001-0000-0000-0000-000000000002', 'Mangalarga Marchador', NOW(), NULL),
    ('00000002-0000-0000-0000-000000000004', '00000001-0000-0000-0000-000000000002', 'Quarto de Milha', NOW(), NULL),

    -- Raças de Bubalinos
    ('00000002-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000003', 'Murrah', NOW(), NULL),
    ('00000002-0000-0000-0000-000000000006', '00000001-0000-0000-0000-000000000003', 'Mediterrâneo', NOW(), NULL),

    -- Raças de Asininos
    ('00000002-0000-0000-0000-000000000007', '00000001-0000-0000-0000-000000000004', 'Pêga', NOW(), NULL),
    ('00000002-0000-0000-0000-000000000008', '00000001-0000-0000-0000-000000000004', 'Paulista', NOW(), NULL),

    -- Raças de Muares
    ('00000002-0000-0000-0000-000000000009', '00000001-0000-0000-0000-000000000005', 'Mula Marchadeira', NOW(), NULL),
    ('00000002-0000-0000-0000-000000000010', '00000001-0000-0000-0000-000000000005', 'Burro de Carga', NOW(), NULL);

-- -----------------------------------------------------------------------------
-- 3. ANIMAIS (5 Animais por Raça = 50 Animais)
-- OrigemAnimal: 0 = Interno, 1 = Externo
-- Sexo: 'Macho' / 'Femea'
-- -----------------------------------------------------------------------------
INSERT INTO "Animals" ("Id", "Name", "RacaId", "DataNascimento", "OrigemAnimal", "LoteOuPasto", "Ativo", "CreationDateTime", "LastModificationDateTime", "Sexo")
VALUES
    -- Raça 1: Nelore (Bovino)
    ('00000003-0000-0000-0000-000000000001', 'Touro Soberano', '00000002-0000-0000-0000-000000000001', '2021-03-15', 0, 'Pasto 01 - Nelore Corte', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000002', 'Vaca Estrela', '00000002-0000-0000-0000-000000000001', '2022-05-10', 0, 'Pasto 01 - Nelore Corte', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000003', 'Bezerro Alvorada', '00000002-0000-0000-0000-000000000001', '2024-01-20', 0, 'Pasto 01 - Nelore Corte', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000004', 'Novilha Cristal', '00000002-0000-0000-0000-000000000001', '2023-08-14', 1, 'Pasto 01 - Nelore Corte', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000005', 'Touro Guardião', '00000002-0000-0000-0000-000000000001', '2021-11-02', 0, 'Pasto 01 - Nelore Corte', true, NOW(), NULL, 'Macho'),

    -- Raça 2: Holandês (Bovino)
    ('00000003-0000-0000-0000-000000000006', 'Vaca Mimosa', '00000002-0000-0000-0000-000000000002', '2020-04-12', 0, 'Pasto 02 - Leiteiro', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000007', 'Vaca Malhada', '00000002-0000-0000-0000-000000000002', '2021-07-25', 0, 'Pasto 02 - Leiteiro', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000008', 'Novilha Princesa', '00000002-0000-0000-0000-000000000002', '2023-02-18', 0, 'Pasto 02 - Leiteiro', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000009', 'Touro Barão Holandês', '00000002-0000-0000-0000-000000000002', '2022-09-30', 1, 'Pasto 02 - Leiteiro', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000010', 'Bezerra Formosa', '00000002-0000-0000-0000-000000000002', '2024-03-05', 0, 'Pasto 02 - Leiteiro', true, NOW(), NULL, 'Femea'),

    -- Raça 3: Mangalarga Marchador (Equino)
    ('00000003-0000-0000-0000-000000000011', 'Garanhão Relâmpago', '00000002-0000-0000-0000-000000000003', '2019-10-10', 0, 'Haras Piquete 01', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000012', 'Égua Morena', '00000002-0000-0000-0000-000000000003', '2020-11-15', 0, 'Haras Piquete 01', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000013', 'Potro Tempestade', '00000002-0000-0000-0000-000000000003', '2023-12-01', 0, 'Haras Piquete 01', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000014', 'Égua Bailarina', '00000002-0000-0000-0000-000000000003', '2021-06-20', 1, 'Haras Piquete 01', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000015', 'Cavalo Faraó', '00000002-0000-0000-0000-000000000003', '2022-01-08', 0, 'Haras Piquete 01', true, NOW(), NULL, 'Macho'),

    -- Raça 4: Quarto de Milha (Equino)
    ('00000003-0000-0000-0000-000000000016', 'Garanhão Trovão Negro', '00000002-0000-0000-0000-000000000004', '2018-08-22', 1, 'Haras Piquete 02', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000017', 'Égua Dourada', '00000002-0000-0000-0000-000000000004', '2020-03-14', 0, 'Haras Piquete 02', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000018', 'Potra Serena', '00000002-0000-0000-0000-000000000004', '2023-09-09', 0, 'Haras Piquete 02', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000019', 'Cavalo Ventania', '00000002-0000-0000-0000-000000000004', '2021-05-19', 0, 'Haras Piquete 02', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000020', 'Cavalo Comanche', '00000002-0000-0000-0000-000000000004', '2022-04-03', 1, 'Haras Piquete 02', true, NOW(), NULL, 'Macho'),

    -- Raça 5: Murrah (Bubalino)
    ('00000003-0000-0000-0000-000000000021', 'Búfalo Nero', '00000002-0000-0000-0000-000000000005', '2021-02-17', 0, 'Pasto 03 - Várzea dos Búfalos', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000022', 'Búfala Pandora', '00000002-0000-0000-0000-000000000005', '2022-06-21', 0, 'Pasto 03 - Várzea dos Búfalos', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000023', 'Búfalo Sansão', '00000002-0000-0000-0000-000000000005', '2020-12-05', 1, 'Pasto 03 - Várzea dos Búfalos', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000024', 'Bezerro Titan', '00000002-0000-0000-0000-000000000005', '2024-02-11', 0, 'Pasto 03 - Várzea dos Búfalos', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000025', 'Búfala Pérola', '00000002-0000-0000-0000-000000000005', '2023-01-30', 0, 'Pasto 03 - Várzea dos Búfalos', true, NOW(), NULL, 'Femea'),

    -- Raça 6: Mediterrâneo (Bubalino)
    ('00000003-0000-0000-0000-000000000026', 'Búfalo Sultão', '00000002-0000-0000-0000-000000000006', '2021-08-14', 0, 'Pasto 04 - Búfalos Sul', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000027', 'Búfala Atena', '00000002-0000-0000-0000-000000000006', '2022-04-18', 0, 'Pasto 04 - Búfalos Sul', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000028', 'Búfala Diana', '00000002-0000-0000-0000-000000000006', '2023-05-22', 1, 'Pasto 04 - Búfalos Sul', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000029', 'Búfalo Zeus', '00000002-0000-0000-0000-000000000006', '2020-10-09', 0, 'Pasto 04 - Búfalos Sul', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000030', 'Bezerra Safira', '00000002-0000-0000-0000-000000000006', '2024-04-01', 0, 'Pasto 04 - Búfalos Sul', true, NOW(), NULL, 'Femea'),

    -- Raça 7: Pêga (Asinino)
    ('00000003-0000-0000-0000-000000000031', 'Jumento Imperial Pêga', '00000002-0000-0000-0000-000000000007', '2019-09-12', 0, 'Piquete Asininos', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000032', 'Jumenta Graciosa', '00000002-0000-0000-0000-000000000007', '2021-01-25', 0, 'Piquete Asininos', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000033', 'Jumento Campeão', '00000002-0000-0000-0000-000000000007', '2022-07-07', 1, 'Piquete Asininos', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000034', 'Jumenta Boneca', '00000002-0000-0000-0000-000000000007', '2023-03-19', 0, 'Piquete Asininos', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000035', 'Jumentinho Fagulha', '00000002-0000-0000-0000-000000000007', '2024-01-15', 0, 'Piquete Asininos', true, NOW(), NULL, 'Macho'),

    -- Raça 8: Paulista (Asinino)
    ('00000003-0000-0000-0000-000000000036', 'Jumento Bandeirante', '00000002-0000-0000-0000-000000000008', '2020-05-30', 0, 'Piquete Asininos 02', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000037', 'Jumenta Paulistana', '00000002-0000-0000-0000-000000000008', '2021-08-11', 0, 'Piquete Asininos 02', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000038', 'Jumento Sertanejo', '00000002-0000-0000-0000-000000000008', '2022-11-20', 1, 'Piquete Asininos 02', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000039', 'Jumenta Moça', '00000002-0000-0000-0000-000000000008', '2023-06-04', 0, 'Piquete Asininos 02', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000040', 'Jumentinho Piratininga', '00000002-0000-0000-0000-000000000008', '2024-02-28', 0, 'Piquete Asininos 02', true, NOW(), NULL, 'Macho'),

    -- Raça 9: Mula Marchadeira (Muar)
    ('00000003-0000-0000-0000-000000000041', 'Mula Dengosa', '00000002-0000-0000-0000-000000000009', '2020-03-08', 0, 'Pasto dos Muares', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000042', 'Mula Serenata', '00000002-0000-0000-0000-000000000009', '2021-09-17', 0, 'Pasto dos Muares', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000043', 'Mula Pretinha', '00000002-0000-0000-0000-000000000009', '2022-04-29', 1, 'Pasto dos Muares', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000044', 'Mula Feiticeira', '00000002-0000-0000-0000-000000000009', '2023-07-13', 0, 'Pasto dos Muares', true, NOW(), NULL, 'Femea'),
    ('00000003-0000-0000-0000-000000000045', 'Mula Querência', '00000002-0000-0000-0000-000000000009', '2023-11-05', 0, 'Pasto dos Muares', true, NOW(), NULL, 'Femea'),

    -- Raça 10: Burro de Carga (Muar)
    ('00000003-0000-0000-0000-000000000046', 'Burro Hércules', '00000002-0000-0000-0000-000000000010', '2019-12-02', 0, 'Pasto dos Muares', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000047', 'Burro Pagador', '00000002-0000-0000-0000-000000000010', '2021-06-16', 0, 'Pasto dos Muares', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000048', 'Burro Diamante', '00000002-0000-0000-0000-000000000010', '2022-08-23', 1, 'Pasto dos Muares', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000049', 'Burro Valente', '00000002-0000-0000-0000-000000000010', '2023-01-11', 0, 'Pasto dos Muares', true, NOW(), NULL, 'Macho'),
    ('00000003-0000-0000-0000-000000000050', 'Burro Guerreiro', '00000002-0000-0000-0000-000000000010', '2023-10-27', 0, 'Pasto dos Muares', true, NOW(), NULL, 'Macho');

-- -----------------------------------------------------------------------------
-- 4. IDENTIFICADORES DOS ANIMAIS (100 Identificadores - 2 para cada um dos 50 Animais)
-- Tipos:
--   0 = BrincoVisual
--   1 = BrincoEletronico (RFID)
--   2 = Sisbov (Rastreabilidade Oficial Bovinos/Bubalinos)
--   3 = Microchip (Equídeos)
--   4 = RegistroAssociado (Registro Genealógico / Associação de Raça)
--   5 = Tatuagem (Tatuagem Auricular ou Labial)
-- -----------------------------------------------------------------------------
INSERT INTO "IdentificadorAnimal" ("Id", "AnimalId", "Tipo", "Valor", "IsPrincipal", "CreationDateTime", "LastModificationDateTime")
VALUES
    -- Nelore (Animal 1 a 5): SISBOV Oficial (Principal) + Tatuagem / Brinco de Manejo (Secundário)
    ('00000005-0000-0000-0000-000000000001', '00000003-0000-0000-0000-000000000001', 2, 'BR076000001001', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000002', '00000003-0000-0000-0000-000000000001', 5, 'TAT-NEL-001', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000003', '00000003-0000-0000-0000-000000000002', 2, 'BR076000001002', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000004', '00000003-0000-0000-0000-000000000002', 0, 'BV-NEL-002', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000005', '00000003-0000-0000-0000-000000000003', 2, 'BR076000001003', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000006', '00000003-0000-0000-0000-000000000003', 5, 'TAT-NEL-003', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000007', '00000003-0000-0000-0000-000000000004', 2, 'BR076000001004', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000008', '00000003-0000-0000-0000-000000000004', 0, 'BV-NEL-004', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000009', '00000003-0000-0000-0000-000000000005', 2, 'BR076000001005', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000010', '00000003-0000-0000-0000-000000000005', 5, 'TAT-NEL-005', false, NOW(), NULL),

    -- Holandês (Animal 6 a 10): Brinco Visual de Manejo Leiteiro (Principal) + RFID / Registro (Secundário)
    ('00000005-0000-0000-0000-000000000011', '00000003-0000-0000-0000-000000000006', 0, 'BV-HOL-101', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000012', '00000003-0000-0000-0000-000000000006', 1, 'RFID-982-000101', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000013', '00000003-0000-0000-0000-000000000007', 0, 'BV-HOL-102', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000014', '00000003-0000-0000-0000-000000000007', 1, 'RFID-982-000102', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000015', '00000003-0000-0000-0000-000000000008', 0, 'BV-HOL-103', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000016', '00000003-0000-0000-0000-000000000008', 1, 'RFID-982-000103', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000017', '00000003-0000-0000-0000-000000000009', 0, 'BV-HOL-104', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000018', '00000003-0000-0000-0000-000000000009', 4, 'RGH-045.891', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000019', '00000003-0000-0000-0000-000000000010', 0, 'BV-HOL-105', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000020', '00000003-0000-0000-0000-000000000010', 1, 'RFID-982-000105', false, NOW(), NULL),

    -- Mangalarga Marchador (Animal 11 a 15): Microchip Nucal (Principal) + Registro ABCCMM (Secundário)
    ('00000005-0000-0000-0000-000000000021', '00000003-0000-0000-0000-000000000011', 3, '981098100000011', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000022', '00000003-0000-0000-0000-000000000011', 4, 'ABCCMM-214.301', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000023', '00000003-0000-0000-0000-000000000012', 3, '981098100000012', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000024', '00000003-0000-0000-0000-000000000012', 4, 'ABCCMM-214.302', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000025', '00000003-0000-0000-0000-000000000013', 3, '981098100000013', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000026', '00000003-0000-0000-0000-000000000013', 4, 'ABCCMM-214.303', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000027', '00000003-0000-0000-0000-000000000014', 3, '981098100000014', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000028', '00000003-0000-0000-0000-000000000014', 4, 'ABCCMM-214.304', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000029', '00000003-0000-0000-0000-000000000015', 3, '981098100000015', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000030', '00000003-0000-0000-0000-000000000015', 4, 'ABCCMM-214.305', false, NOW(), NULL),

    -- Quarto de Milha (Animal 16 a 20): Microchip Nucal (Principal) + Registro ABQM (Secundário)
    ('00000005-0000-0000-0000-000000000031', '00000003-0000-0000-0000-000000000016', 3, '981098100000016', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000032', '00000003-0000-0000-0000-000000000016', 4, 'ABQM-P189401', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000033', '00000003-0000-0000-0000-000000000017', 3, '981098100000017', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000034', '00000003-0000-0000-0000-000000000017', 4, 'ABQM-P189402', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000035', '00000003-0000-0000-0000-000000000018', 3, '981098100000018', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000036', '00000003-0000-0000-0000-000000000018', 4, 'ABQM-P189403', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000037', '00000003-0000-0000-0000-000000000019', 3, '981098100000019', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000038', '00000003-0000-0000-0000-000000000019', 4, 'ABQM-P189404', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000039', '00000003-0000-0000-0000-000000000020', 3, '981098100000020', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000040', '00000003-0000-0000-0000-000000000020', 4, 'ABQM-P189405', false, NOW(), NULL),

    -- Murrah (Animal 21 a 25): SISBOV Bubalino (Principal) + Tatuagem Auricular (Secundário)
    ('00000005-0000-0000-0000-000000000041', '00000003-0000-0000-0000-000000000021', 2, 'BR076000002021', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000042', '00000003-0000-0000-0000-000000000021', 5, 'TAT-BUF-021', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000043', '00000003-0000-0000-0000-000000000022', 2, 'BR076000002022', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000044', '00000003-0000-0000-0000-000000000022', 5, 'TAT-BUF-022', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000045', '00000003-0000-0000-0000-000000000023', 2, 'BR076000002023', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000046', '00000003-0000-0000-0000-000000000023', 5, 'TAT-BUF-023', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000047', '00000003-0000-0000-0000-000000000024', 2, 'BR076000002024', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000048', '00000003-0000-0000-0000-000000000024', 5, 'TAT-BUF-024', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000049', '00000003-0000-0000-0000-000000000025', 2, 'BR076000002025', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000050', '00000003-0000-0000-0000-000000000025', 5, 'TAT-BUF-025', false, NOW(), NULL),

    -- Mediterrâneo (Animal 26 a 30): Brinco Visual (Principal) + RFID Auricular (Secundário)
    ('00000005-0000-0000-0000-000000000051', '00000003-0000-0000-0000-000000000026', 0, 'BV-MED-201', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000052', '00000003-0000-0000-0000-000000000026', 1, 'RFID-982-000201', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000053', '00000003-0000-0000-0000-000000000027', 0, 'BV-MED-202', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000054', '00000003-0000-0000-0000-000000000027', 1, 'RFID-982-000202', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000055', '00000003-0000-0000-0000-000000000028', 0, 'BV-MED-203', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000056', '00000003-0000-0000-0000-000000000028', 1, 'RFID-982-000203', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000057', '00000003-0000-0000-0000-000000000029', 0, 'BV-MED-204', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000058', '00000003-0000-0000-0000-000000000029', 1, 'RFID-982-000204', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000059', '00000003-0000-0000-0000-000000000030', 0, 'BV-MED-205', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000060', '00000003-0000-0000-0000-000000000030', 1, 'RFID-982-000205', false, NOW(), NULL),

    -- Pêga (Animal 31 a 35): Registro Associação ABCPêga (Principal) + Microchip (Secundário)
    ('00000005-0000-0000-0000-000000000061', '00000003-0000-0000-0000-000000000031', 4, 'ABCPEGA-12301', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000062', '00000003-0000-0000-0000-000000000031', 3, '981098100000031', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000063', '00000003-0000-0000-0000-000000000032', 4, 'ABCPEGA-12302', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000064', '00000003-0000-0000-0000-000000000032', 3, '981098100000032', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000065', '00000003-0000-0000-0000-000000000033', 4, 'ABCPEGA-12303', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000066', '00000003-0000-0000-0000-000000000033', 3, '981098100000033', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000067', '00000003-0000-0000-0000-000000000034', 4, 'ABCPEGA-12304', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000068', '00000003-0000-0000-0000-000000000034', 3, '981098100000034', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000069', '00000003-0000-0000-0000-000000000035', 4, 'ABCPEGA-12305', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000070', '00000003-0000-0000-0000-000000000035', 3, '981098100000035', false, NOW(), NULL),

    -- Paulista (Animal 36 a 40): Tatuagem Auricular (Principal) + Brinco de Manejo (Secundário)
    ('00000005-0000-0000-0000-000000000071', '00000003-0000-0000-0000-000000000036', 5, 'TAT-JUM-036', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000072', '00000003-0000-0000-0000-000000000036', 0, 'BV-JUM-036', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000073', '00000003-0000-0000-0000-000000000037', 5, 'TAT-JUM-037', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000074', '00000003-0000-0000-0000-000000000037', 0, 'BV-JUM-037', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000075', '00000003-0000-0000-0000-000000000038', 5, 'TAT-JUM-038', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000076', '00000003-0000-0000-0000-000000000038', 0, 'BV-JUM-038', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000077', '00000003-0000-0000-0000-000000000039', 5, 'TAT-JUM-039', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000078', '00000003-0000-0000-0000-000000000039', 0, 'BV-JUM-039', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000079', '00000003-0000-0000-0000-000000000040', 5, 'TAT-JUM-040', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000080', '00000003-0000-0000-0000-000000000040', 0, 'BV-JUM-040', false, NOW(), NULL),

    -- Mula Marchadeira (Animal 41 a 45): Microchip (Principal) + Brinco Visual (Secundário)
    ('00000005-0000-0000-0000-000000000081', '00000003-0000-0000-0000-000000000041', 3, '981098100000041', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000082', '00000003-0000-0000-0000-000000000041', 0, 'BV-MUL-041', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000083', '00000003-0000-0000-0000-000000000042', 3, '981098100000042', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000084', '00000003-0000-0000-0000-000000000042', 0, 'BV-MUL-042', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000085', '00000003-0000-0000-0000-000000000043', 3, '981098100000043', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000086', '00000003-0000-0000-0000-000000000043', 0, 'BV-MUL-043', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000087', '00000003-0000-0000-0000-000000000044', 3, '981098100000044', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000088', '00000003-0000-0000-0000-000000000044', 0, 'BV-MUL-044', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000089', '00000003-0000-0000-0000-000000000045', 3, '981098100000045', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000090', '00000003-0000-0000-0000-000000000045', 0, 'BV-MUL-045', false, NOW(), NULL),

    -- Burro de Carga (Animal 46 a 50): Brinco Visual (Principal) + Marcação / Tatuagem (Secundário)
    ('00000005-0000-0000-0000-000000000091', '00000003-0000-0000-0000-000000000046', 0, 'BV-BUR-046', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000092', '00000003-0000-0000-0000-000000000046', 5, 'TAT-BUR-046', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000093', '00000003-0000-0000-0000-000000000047', 0, 'BV-BUR-047', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000094', '00000003-0000-0000-0000-000000000047', 5, 'TAT-BUR-047', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000095', '00000003-0000-0000-0000-000000000048', 0, 'BV-BUR-048', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000096', '00000003-0000-0000-0000-000000000048', 5, 'TAT-BUR-048', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000097', '00000003-0000-0000-0000-000000000049', 0, 'BV-BUR-049', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000098', '00000003-0000-0000-0000-000000000049', 5, 'TAT-BUR-049', false, NOW(), NULL),

    ('00000005-0000-0000-0000-000000000099', '00000003-0000-0000-0000-000000000050', 0, 'BV-BUR-050', true, NOW(), NULL),
    ('00000005-0000-0000-0000-000000000100', '00000003-0000-0000-0000-000000000050', 5, 'TAT-BUR-050', false, NOW(), NULL);

-- -----------------------------------------------------------------------------
-- 5. VACINAS (20 Vacinas)
-- -----------------------------------------------------------------------------
INSERT INTO "Vacinas" ("Id", "EspecieId", "Name", "Descricao", "ReaplicarEmXDias", "ObrigatorioOrgaoSanitario", "CreationDateTime", "LastModificationDateTime")
VALUES
    -- Vacinas Bovinas
    ('00000004-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000001', 'Vacina Contra Febre Aftosa (Bivalente O e A)', 'Imunização obrigatória contra o vírus da febre aftosa para rebanhos bovinos.', 180, true, NOW(), NULL),
    ('00000004-0000-0000-0000-000000000002', '00000001-0000-0000-0000-000000000001', 'Vacina Contra Brucelose B19', 'Vacina viva atenuada obrigatória para bezerras entre 3 e 8 meses de idade.', 365, true, NOW(), NULL),
    ('00000004-0000-0000-0000-000000000003', '00000001-0000-0000-0000-000000000001', 'Vacina Contra Brucelose RB51', 'Vacina não aglutinogênica indicada para fêmeas adultas não vacinadas com B19.', 365, false, NOW(), NULL),
    ('00000004-0000-0000-0000-000000000004', '00000001-0000-0000-0000-000000000001', 'Vacina Polivalente contra Clostridioses (10 vias)', 'Proteção contra carbúnculo sintomático, gangrena gasosa, enterotoxemia e tétano.', 365, false, NOW(), NULL),
    ('00000004-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000001', 'Vacina Contra Raiva dos Herbívoros', 'Profilaxia da raiva transmitida por morcegos hematófagos em bovinos.', 365, true, NOW(), NULL),
    ('00000004-0000-0000-0000-000000000006', '00000001-0000-0000-0000-000000000001', 'Vacina Reprodutiva contra IBR e BVD', 'Prevenção de Rinotraqueíte Infecciosa Bovina e Diarreia Viral Bovina.', 365, false, NOW(), NULL),
    ('00000004-0000-0000-0000-000000000007', '00000001-0000-0000-0000-000000000001', 'Vacina Contra Leptospirose Bovina', 'Prevenção contra os principais sorovares de Leptospira interrogans em bovinos.', 180, false, NOW(), NULL),
    ('00000004-0000-0000-0000-000000000008', '00000001-0000-0000-0000-000000000001', 'Vacina Contra Carbúnculo Hemático (Antrax)', 'Imunização contra Bacillus anthracis em áreas com histórico da doença.', 365, true, NOW(), NULL),

    -- Vacinas Equinas
    ('00000004-0000-0000-0000-000000000009', '00000001-0000-0000-0000-000000000002', 'Vacina Contra Encefalomielite Equina (Leste e Oeste)', 'Prevenção contra o alphavirus causador da encefalomielite viral em equinos.', 365, false, NOW(), NULL),
    ('00000004-0000-0000-0000-000000000010', '00000001-0000-0000-0000-000000000002', 'Vacina Contra Influenza Equina (Gripe Equina)', 'Proteção contra cepas de Myxovirus influenzae A/equi 1 e A/equi 2.', 180, false, NOW(), NULL),
    ('00000004-0000-0000-0000-000000000011', '00000001-0000-0000-0000-000000000002', 'Vacina Contra Tétano Equino (Toxóide Tetânico)', 'Imunização ativa contra Clostridium tetani em cavalos de esporte e trabalho.', 365, false, NOW(), NULL),
    ('00000004-0000-0000-0000-000000000012', '00000001-0000-0000-0000-000000000002', 'Vacina Contra Rinopneumonite Equina (EHV-1 e EHV-4)', 'Prevenção de aborto viral e distúrbios respiratórios agudos em equinos.', 180, false, NOW(), NULL),
    ('00000004-0000-0000-0000-000000000013', '00000001-0000-0000-0000-000000000002', 'Vacina Contra Raiva dos Equinos', 'Imunização contra o vírus rábico indicada para equinos em áreas rurais.', 365, true, NOW(), NULL),
    ('00000004-0000-0000-0000-000000000014', '00000001-0000-0000-0000-000000000002', 'Vacina Contra Garrotilho Equino', 'Prevenção da adenite equina (Streptococcus equi) em potros e adultos.', 365, false, NOW(), NULL),

    -- Vacinas Bubalinas
    ('00000004-0000-0000-0000-000000000015', '00000001-0000-0000-0000-000000000003', 'Vacina Contra Clostridiose Bubalina', 'Prevenção específica de enterotoxemias e carbúnculo em rebanhos bubalinos.', 365, false, NOW(), NULL),
    ('00000004-0000-0000-0000-000000000016', '00000001-0000-0000-0000-000000000003', 'Vacina Contra Botulismo Bubalino (C e D)', 'Proteção contra intoxicação botulínica em búfalos em áreas alagadas.', 365, false, NOW(), NULL),

    -- Vacinas Asininas e Muares
    ('00000004-0000-0000-0000-000000000017', '00000001-0000-0000-0000-000000000004', 'Vacina Contra Raiva para Asininos', 'Profilaxia contra raiva em jumentos e asininos reprodutores.', 365, true, NOW(), NULL),
    ('00000004-0000-0000-0000-000000000018', '00000001-0000-0000-0000-000000000005', 'Vacina Contra Tétano e Clostridioses para Muares', 'Toxóide tetânico e bacterina clostridial purificada para mulas e burros de carga.', 365, false, NOW(), NULL),

    -- Vacinas Gerais / Multiespécie (EspecieId = NULL)
    ('00000004-0000-0000-0000-000000000019', NULL, 'Vacina Polivalente de Grande Porte (Multiespécie)', 'Formulação imunoprofilática ampla aplicável a bovinos, bubalinos e equídeos.', 365, false, NOW(), NULL),
    ('00000004-0000-0000-0000-000000000020', NULL, 'Vacina Imunoprofilática Amplo Espectro', 'Reforço imunitário geral contra patógenos oportunistas em animais de grande porte.', 365, false, NOW(), NULL);

-- -----------------------------------------------------------------------------
-- 6. DATAS DE NASCIMENTO APROXIMADAS
-- -----------------------------------------------------------------------------
UPDATE "Animals"
SET "DataNascimentoAproximada" = true
WHERE "Id" IN (
    '00000003-0000-0000-0000-000000000003',
    '00000003-0000-0000-0000-000000000013',
    '00000003-0000-0000-0000-000000000024',
    '00000003-0000-0000-0000-000000000035',
    '00000003-0000-0000-0000-000000000045'
);

-- -----------------------------------------------------------------------------
-- 7. PROTOCOLOS VACINAIS
-- -----------------------------------------------------------------------------
INSERT INTO "ProtocolosVacinais" ("Id", "Nome", "Descricao", "Finalidade", "EspecieId", "Ativo", "IdadeMinimaDias", "IdadeMaximaDias", "CreationDateTime", "LastModificationDateTime")
VALUES
    ('00000007-0000-0000-0000-000000000001', 'Protocolo Bovino de Entrada', 'Esquema inicial para bovinos recém-integrados ao plantel.', 'Entrada no plantel', '00000001-0000-0000-0000-000000000001', true, NULL, NULL, NOW(), NULL),
    ('00000007-0000-0000-0000-000000000002', 'Protocolo Equino Sanitário', 'Esquema básico para equinos de trabalho, reprodução e esporte.', 'Rotina sanitária', '00000001-0000-0000-0000-000000000002', true, NULL, NULL, NOW(), NULL),
    ('00000007-0000-0000-0000-000000000003', 'Protocolo Multiespécie de Reforço', 'Esquema genérico para animais de grande porte quando não há protocolo específico.', 'Reforço geral', NULL, true, NULL, NULL, NOW(), NULL);

INSERT INTO "ProtocoloVacinalDoses" ("Id", "ProtocoloVacinalId", "VacinaId", "Ordem", "DiasAposInicio", "IntervaloReforcoDias", "Obrigatoria", "Observacoes", "CreationDateTime", "LastModificationDateTime")
VALUES
    ('00000008-0000-0000-0000-000000000001', '00000007-0000-0000-0000-000000000001', '00000004-0000-0000-0000-000000000004', 1, 0, 365, true, 'Aplicar na entrada ou no primeiro manejo sanitário.', NOW(), NULL),
    ('00000008-0000-0000-0000-000000000002', '00000007-0000-0000-0000-000000000001', '00000004-0000-0000-0000-000000000002', 2, 90, NULL, true, 'Indicada conforme faixa etária e orientação veterinária.', NOW(), NULL),
    ('00000008-0000-0000-0000-000000000003', '00000007-0000-0000-0000-000000000001', '00000004-0000-0000-0000-000000000005', 3, 180, 365, false, 'Reforço anual quando aplicável.', NOW(), NULL),

    ('00000008-0000-0000-0000-000000000004', '00000007-0000-0000-0000-000000000002', '00000004-0000-0000-0000-000000000009', 1, 0, 365, true, 'Primeira dose do protocolo equino.', NOW(), NULL),
    ('00000008-0000-0000-0000-000000000005', '00000007-0000-0000-0000-000000000002', '00000004-0000-0000-0000-000000000010', 2, 30, 180, true, 'Reforço semestral conforme calendário sanitário.', NOW(), NULL),

    ('00000008-0000-0000-0000-000000000006', '00000007-0000-0000-0000-000000000003', '00000004-0000-0000-0000-000000000019', 1, 0, 365, true, 'Dose de cobertura para animais sem protocolo específico.', NOW(), NULL),
    ('00000008-0000-0000-0000-000000000007', '00000007-0000-0000-0000-000000000003', '00000004-0000-0000-0000-000000000020', 2, 30, NULL, false, 'Dose complementar de acordo com avaliação veterinária.', NOW(), NULL);

-- -----------------------------------------------------------------------------
-- 8. ATRIBUIÇÃO DE PROTOCOLOS A ANIMAIS
-- -----------------------------------------------------------------------------
INSERT INTO "AnimalProtocolosVacinais" ("Id", "AnimalId", "ProtocoloVacinalId", "DataInicio", "Ativo", "Observacoes", "CreationDateTime", "LastModificationDateTime")
VALUES
    ('00000009-0000-0000-0000-000000000001', '00000003-0000-0000-0000-000000000001', '00000007-0000-0000-0000-000000000001', '2025-01-10', true, 'Protocolo iniciado na entrada do animal no plantel.', NOW(), NULL),
    ('00000009-0000-0000-0000-000000000002', '00000003-0000-0000-0000-000000000006', '00000007-0000-0000-0000-000000000001', '2025-02-15', true, NULL, NOW(), NULL),
    ('00000009-0000-0000-0000-000000000003', '00000003-0000-0000-0000-000000000011', '00000007-0000-0000-0000-000000000002', '2025-03-20', true, 'Animal de trabalho.', NOW(), NULL),
    ('00000009-0000-0000-0000-000000000004', '00000003-0000-0000-0000-000000000016', '00000007-0000-0000-0000-000000000002', '2025-04-05', true, NULL, NOW(), NULL),
    ('00000009-0000-0000-0000-000000000005', '00000003-0000-0000-0000-000000000021', '00000007-0000-0000-0000-000000000003', '2025-01-25', true, 'Protocolo multiespécie para bubalino.', NOW(), NULL),
    ('00000009-0000-0000-0000-000000000006', '00000003-0000-0000-0000-000000000031', '00000007-0000-0000-0000-000000000003', '2025-05-12', true, NULL, NOW(), NULL);

-- -----------------------------------------------------------------------------
-- 9. HISTÓRICO DE MOVIMENTAÇÃO
-- Uma entrada inicial é criada para cada animal usando o lote/pasto atual.
-- O UUID é determinístico para manter a relação estável em ambientes de teste.
-- TipoMovimentacaoAnimal: 0 = Entrada, 1 = Transferencia, 2 = Saida, 3 = Ajuste
-- -----------------------------------------------------------------------------
INSERT INTO "MovimentacoesAnimais" ("Id", "AnimalId", "DataMovimentacao", "Tipo", "LocalOrigem", "LocalDestino", "Motivo", "Observacoes", "CreationDateTime", "LastModificationDateTime")
SELECT
    md5('movimentacao-entrada-' || a."Id"::text)::uuid,
    a."Id",
    a."CreationDateTime"::date,
    0,
    NULL,
    a."LoteOuPasto",
    'Entrada inicial no plantel',
    'Registro criado pelo seed.',
    a."CreationDateTime",
    NULL
FROM "Animals" a;

INSERT INTO "MovimentacoesAnimais" ("Id", "AnimalId", "DataMovimentacao", "Tipo", "LocalOrigem", "LocalDestino", "Motivo", "Observacoes", "CreationDateTime", "LastModificationDateTime")
VALUES
    ('00000006-0000-0000-0000-000000000001', '00000003-0000-0000-0000-000000000001', '2025-06-15', 1, 'Pasto 01 - Nelore Corte', 'Pasto de Manejo Sanitário', 'Manejo sanitário', 'Transferência temporária para vacinação.', NOW(), NULL),
    ('00000006-0000-0000-0000-000000000002', '00000003-0000-0000-0000-000000000011', '2025-07-02', 1, 'Haras Piquete 01', 'Baia de Observação', 'Observação clínica', NULL, NOW(), NULL),
    ('00000006-0000-0000-0000-000000000003', '00000003-0000-0000-0000-000000000021', '2025-08-10', 3, 'Pasto 03 - Várzea dos Búfalos', 'Pasto 03 - Várzea dos Búfalos', 'Ajuste cadastral', 'Correção do local atual.', NOW(), NULL);

-- -----------------------------------------------------------------------------
-- 10. APLICAÇÕES DE VACINA
-- StatusComprovanteVacina: 0 = NaoEmitido, 1 = PendenteAssinatura, 2 = Assinado
-- -----------------------------------------------------------------------------
INSERT INTO "AplicacoesVacina" ("Id", "VacinaId", "AnimalId", "AnimalProtocoloVacinalId", "ProtocoloVacinalDoseId", "DataAplicacao", "DataProximaDose", "CicloFinalizado", "StatusComprovante", "NumeroLote", "DoseMl", "DataFabricacao", "DataValidade", "LocalAplicacao", "Observacoes", "VeterinarioResponsavel", "VeterinarioCrmv", "VeterinarioCrmvUf", "Aplicador", "LaboratorioFabricante", "AssinadaEm", "Assinante", "AssinanteCrmv", "AssinanteCrmvUf", "HashAssinatura", "MetodoAssinatura", "CreationDateTime", "LastModificationDateTime")
VALUES
    ('0000000a-0000-0000-0000-000000000001', '00000004-0000-0000-0000-000000000004', '00000003-0000-0000-0000-000000000001', '00000009-0000-0000-0000-000000000001', '00000008-0000-0000-0000-000000000001', '2025-01-10', '2026-01-10', false, 1, 'LOT-SEED-BOV-001', '5.0', '2024-08-01', '2026-08-01', 'Pasto de Manejo Sanitário', 'Dose inicial do protocolo bovino.', 'Dra. Ana Ribeiro', '10001', 'AM', 'Equipe de Campo', 'Laboratório VetBio', NULL, NULL, NULL, NULL, NULL, NULL, NOW(), NULL),
    ('0000000a-0000-0000-0000-000000000002', '00000004-0000-0000-0000-000000000002', '00000003-0000-0000-0000-000000000001', '00000009-0000-0000-0000-000000000001', '00000008-0000-0000-0000-000000000002', '2025-04-10', NULL, true, 2, 'LOT-SEED-BRU-001', '2.0', '2024-09-10', '2026-09-10', 'Pasto de Manejo Sanitário', 'Dose de acordo com a idade do animal.', 'Dra. Ana Ribeiro', '10001', 'AM', 'Equipe de Campo', 'Laboratório VetBio', '2025-04-10 14:30:00+00', 'Dra. Ana Ribeiro', '10001', 'AM', '1111111111111111111111111111111111111111111111111111111111111111', 'InternaAuditavel', NOW(), NULL),
    ('0000000a-0000-0000-0000-000000000003', '00000004-0000-0000-0000-000000000009', '00000003-0000-0000-0000-000000000011', '00000009-0000-0000-0000-000000000003', '00000008-0000-0000-0000-000000000004', '2025-03-20', '2026-03-20', false, 1, 'LOT-SEED-EQU-001', '3.0', '2024-12-20', '2026-12-20', 'Haras Piquete 01', 'Primeira dose equina.', 'Dr. Carlos Menezes', '10002', 'AM', 'Equipe de Campo', 'Laboratório EquiVet', NULL, NULL, NULL, NULL, NULL, NULL, NOW(), NULL),
    ('0000000a-0000-0000-0000-000000000004', '00000004-0000-0000-0000-000000000010', '00000003-0000-0000-0000-000000000011', '00000009-0000-0000-0000-000000000003', '00000008-0000-0000-0000-000000000005', '2025-04-19', '2025-10-16', false, 1, 'LOT-SEED-INF-001', '2.5', '2024-11-19', '2026-11-19', 'Haras Piquete 01', 'Reforço de influenza pendente de assinatura.', 'Dr. Carlos Menezes', '10002', 'AM', 'Equipe de Campo', 'Laboratório EquiVet', NULL, NULL, NULL, NULL, NULL, NULL, NOW(), NULL),
    ('0000000a-0000-0000-0000-000000000005', '00000004-0000-0000-0000-000000000019', '00000003-0000-0000-0000-000000000021', '00000009-0000-0000-0000-000000000005', '00000008-0000-0000-0000-000000000006', '2025-01-25', '2026-01-25', false, 1, 'LOT-SEED-MUL-001', '4.0', '2024-07-25', '2026-07-25', 'Pasto 03 - Várzea dos Búfalos', 'Cobertura multiespécie.', 'Dra. Ana Ribeiro', '10001', 'AM', 'Equipe de Campo', 'Laboratório VetBio', NULL, NULL, NULL, NULL, NULL, NULL, NOW(), NULL),
    ('0000000a-0000-0000-0000-000000000006', '00000004-0000-0000-0000-000000000019', '00000003-0000-0000-0000-000000000031', NULL, NULL, '2025-05-12', '2026-05-12', false, 1, 'LOT-SEED-AVU-001', '4.0', '2024-10-12', '2026-10-12', 'Pasto 02', 'Aplicação avulsa para demonstração.', 'Dra. Ana Ribeiro', '10001', 'AM', 'Equipe de Campo', 'Laboratório VetBio', NULL, NULL, NULL, NULL, NULL, NULL, NOW(), NULL);

INSERT INTO "AssinaturasAplicacoesVacina" ("Id", "AplicacaoVacinaId", "TipoEvento", "NomeAssinante", "Crmv", "CrmvUf", "DataHora", "HashConteudo", "Metodo", "Observacoes")
VALUES
    ('0000000b-0000-0000-0000-000000000001', '0000000a-0000-0000-0000-000000000002', 'Assinatura', 'Dra. Ana Ribeiro', '10001', 'AM', '2025-04-10 14:30:00+00', '1111111111111111111111111111111111111111111111111111111111111111', 'InternaAuditavel', 'Assinatura interna criada pelo seed.');

COMMIT;
