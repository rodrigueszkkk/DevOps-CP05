-- ============================================================================
-- DISCIPLINA: DevOps Tools & Cloud Computing (Prof. João Menk)
-- ATIVIDADE: 2º Checkpoint - 2º Semestre - Aplicações e Banco em Nuvem
-- PROJETO: SafeShelter - Gestão de Alertas e Dispositivos em Comunidades
-- BANCO: Azure SQL Server (PaaS - db-safeshelter)
-- INTEGRANTES: Kaiky Pereira (RM 564578), Leandro Guarido (RM 561760), Gabriel Solano (RM 562325)
-- ============================================================================

-- 1. TABELA PAI: Comunidades
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Comunidades' AND xtype='U')
BEGIN
    CREATE TABLE Comunidades (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nome VARCHAR(150) NOT NULL,
        PoligonoGeografico VARCHAR(500) NOT NULL,
        CriadoEm DATETIME DEFAULT (SYSDATETIMEOFFSET() AT TIME ZONE 'E. South America Standard Time')
    );
    PRINT 'Tabela Comunidades criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela Comunidades já existe.';
END;
GO

-- 2. TABELA FILHA 1: Dispositivos (Relacionamento 1:N com Comunidades)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Dispositivos' AND xtype='U')
BEGIN
    CREATE TABLE Dispositivos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        MacAddress VARCHAR(50) NOT NULL UNIQUE,
        PerfilResponsavel VARCHAR(100) NOT NULL,
        ComunidadeId INT NOT NULL,
        CriadoEm DATETIME DEFAULT (SYSDATETIMEOFFSET() AT TIME ZONE 'E. South America Standard Time'),
        CONSTRAINT FK_Dispositivos_Comunidades FOREIGN KEY (ComunidadeId) 
            REFERENCES Comunidades(Id) ON DELETE CASCADE
    );
    PRINT 'Tabela Dispositivos criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela Dispositivos já existe.';
END;
GO

-- 3. TABELA FILHA 2: SosLogs (Relacionamento 1:N com Dispositivos)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SosLogs' AND xtype='U')
BEGIN
    CREATE TABLE SosLogs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TipoAlerta VARCHAR(80) NOT NULL,
        Latitude FLOAT NOT NULL,
        Longitude FLOAT NOT NULL,
        Timestamp DATETIME DEFAULT (SYSDATETIMEOFFSET() AT TIME ZONE 'E. South America Standard Time'),
        Status VARCHAR(40) NOT NULL DEFAULT 'PENDENTE',
        DispositivoId INT NOT NULL,
        CONSTRAINT FK_SosLogs_Dispositivos FOREIGN KEY (DispositivoId) 
            REFERENCES Dispositivos(Id) ON DELETE CASCADE
    );
    PRINT 'Tabela SosLogs criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela SosLogs já existe.';
END;
GO

-- 4. CARGA INICIAL PARA TESTES DE PERSISTÊNCIA NO VÍDEO
IF NOT EXISTS (SELECT 1 FROM Comunidades WHERE Nome = 'Comunidade Paraisópolis - Setor A')
BEGIN
    INSERT INTO Comunidades (Nome, PoligonoGeografico) VALUES
        ('Comunidade Paraisópolis - Setor A', '-23.5988,-46.7262;-23.5995,-46.7270'),
        ('Comunidade Heliópolis - Núcleo Central', '-23.6152,-46.5925;-23.6160,-46.5935'),
        ('Comunidade Vila Baquirivu - Encosta Norte', '-23.4560,-46.5120;-23.4570,-46.5130');

    PRINT 'Comunidades iniciais inseridas.';
END;
GO

IF NOT EXISTS (SELECT 1 FROM Dispositivos WHERE MacAddress = 'ESP32-A1B2-C3D4')
BEGIN
    DECLARE @ComunidadeId1 INT = (SELECT TOP 1 Id FROM Comunidades WHERE Nome = 'Comunidade Paraisópolis - Setor A');
    DECLARE @ComunidadeId2 INT = (SELECT TOP 1 Id FROM Comunidades WHERE Nome = 'Comunidade Heliópolis - Núcleo Central');

    INSERT INTO Dispositivos (MacAddress, PerfilResponsavel, ComunidadeId) VALUES
        ('ESP32-A1B2-C3D4', 'Defesa Civil - Setor Sul', @ComunidadeId1),
        ('ESP32-E5F6-G7H8', 'Líder Comunitário Local', @ComunidadeId1),
        ('ESP32-RM561760', 'Equipe de Resgate Rápido', @ComunidadeId2);

    PRINT 'Dispositivos iniciais inseridos.';
END;
GO

IF NOT EXISTS (SELECT 1 FROM SosLogs WHERE TipoAlerta = 'ENCHENTE')
BEGIN
    DECLARE @DispId INT = (SELECT TOP 1 Id FROM Dispositivos WHERE MacAddress = 'ESP32-A1B2-C3D4');

    INSERT INTO SosLogs (TipoAlerta, Latitude, Longitude, Status, DispositivoId) VALUES
        ('ENCHENTE', -23.5989, -46.7263, 'PENDENTE', @DispId),
        ('DESLIZAMENTO', -23.5991, -46.7265, 'ALERTA_CRITICO', @DispId);

    PRINT 'SosLogs iniciais inseridos.';
END;
GO
