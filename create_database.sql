-- ============================================
-- Adventure in the Fantasy World - Database Setup
-- ============================================

-- Drop existing tables if any
DROP TABLE IF EXISTS Characters;
DROP TABLE IF EXISTS Abilities;
DROP TABLE IF EXISTS AbilityTypes;
DROP TABLE IF EXISTS Traps;
DROP TABLE IF EXISTS BuffTypes;
DROP TABLE IF EXISTS Buffs;

-- ============================================
-- Create AbilityTypes table
-- ============================================
CREATE TABLE AbilityTypes (
    abilityTypeId INTEGER PRIMARY KEY,
    abilityTypeName TEXT NOT NULL
);

-- Insert AbilityTypes
INSERT INTO AbilityTypes (abilityTypeId, abilityTypeName) VALUES (1, 'MeleeAttack');
INSERT INTO AbilityTypes (abilityTypeId, abilityTypeName) VALUES (2, 'RangedAttack');
INSERT INTO AbilityTypes (abilityTypeId, abilityTypeName) VALUES (3, 'DefensiveAbility');
INSERT INTO AbilityTypes (abilityTypeId, abilityTypeName) VALUES (4, 'HealingAbility');

-- ============================================
-- Create Characters table
-- ============================================
CREATE TABLE Characters (
    characterId INTEGER PRIMARY KEY,
    characterName TEXT NOT NULL UNIQUE,
    characterHealth REAL NOT NULL,
    characterMoveSpeed REAL NOT NULL,
    characterAttackDamage REAL NOT NULL,
    characterAttackRange REAL NOT NULL,
    characterAttackCooldown REAL NOT NULL
);

-- Insert Characters with correct data
INSERT INTO Characters VALUES (1, 'PlayerCharacter', 150, 250, 25, 1.5, 1);
INSERT INTO Characters VALUES (2, 'testEnemyCharacter', 100, 150, 10, 1, 2);
INSERT INTO Characters VALUES (3, 'testBossCharacter', 300, 75, 30, 1.9, 3);
INSERT INTO Characters VALUES (4, 'SmallSlime', 50, 175, 10, 0.8, 1.2);
INSERT INTO Characters VALUES (5, 'BigSlime', 150, 50, 20, 1.3, 1.9);
INSERT INTO Characters VALUES (6, 'Skeleton', 100, 240, 15, 1.3, 1.8);
INSERT INTO Characters VALUES (7, 'DeathBoss', 400, 200, 40, 3.5, 3);
INSERT INTO Characters VALUES (8, 'DeathGhost', 20, 220, 40, 2, 5);
INSERT INTO Characters VALUES (9, 'DeathGhost(Clone)', 25, 210, 38, 2, 5);
INSERT INTO Characters VALUES (10, 'BlueSkeleton', 150, 190, 30, 2, 1.9);
INSERT INTO Characters VALUES (11, 'RedDeathBoss', 200, 260, 45, 3.6, 3);
INSERT INTO Characters VALUES (12, 'RedDeathGhost', 20, 250, 42, 2, 5);
INSERT INTO Characters VALUES (13, 'RedDeathGhost(Clone)', 20, 250, 42, 2, 5);

-- ============================================
-- Create Abilities table
-- Note: The columns are: abilityId, abilityName, abilityTypeId, abilityKeyCode, abilityCooldown, 
--       attackDamage, abilityRange, projectileSpeed, damageReduction, abilityDuration, healingAmount
-- ============================================
CREATE TABLE Abilities (
    abilityId INTEGER PRIMARY KEY,
    abilityName TEXT NOT NULL UNIQUE,
    abilityTypeId INTEGER NOT NULL,
    abilityKeyCode TEXT DEFAULT '',
    abilityCooldown REAL NOT NULL,
    attackDamage REAL DEFAULT NULL,
    abilityRange REAL DEFAULT NULL,
    projectileSpeed REAL DEFAULT NULL,
    damageReduction INTEGER DEFAULT NULL,
    abilityDuration REAL DEFAULT NULL,
    healingAmount REAL DEFAULT NULL,
    FOREIGN KEY (abilityTypeId) REFERENCES AbilityTypes(abilityTypeId)
);

-- Insert Abilities with correct data
INSERT INTO Abilities VALUES (1, 'FireAttack', 1, 'q', 5, 50, 3, NULL, NULL, NULL, NULL);
INSERT INTO Abilities VALUES (2, 'RangedAttack', 2, 'e', 3, 20, 6, 25, NULL, NULL, NULL);
INSERT INTO Abilities VALUES (3, 'DefensiveAbility', 3, 'f', 7, NULL, NULL, NULL, 25, 5, NULL);
INSERT INTO Abilities VALUES (4, 'HealingAbility', 4, 'g', 10, NULL, NULL, NULL, NULL, NULL, 50);
INSERT INTO Abilities VALUES (5, 'Summon', 2, '*', 4, NULL, 4, NULL, NULL, NULL, NULL);

-- ============================================
-- Create BuffTypes table
-- ============================================
CREATE TABLE BuffTypes (
    buffTypeId INTEGER PRIMARY KEY,
    buffTypeName TEXT NOT NULL
);

INSERT INTO BuffTypes VALUES (1, 'AttackDamageUp');
INSERT INTO BuffTypes VALUES (2, 'DefenseUp');
INSERT INTO BuffTypes VALUES (3, 'AbilityRangeUp');

-- ============================================
-- Create Buffs table
-- ============================================
CREATE TABLE Buffs (
    buffId INTEGER PRIMARY KEY,
    buffName TEXT NOT NULL,
    buffTypeId INTEGER NOT NULL,
    buffAmount REAL NOT NULL,
    FOREIGN KEY (buffTypeId) REFERENCES BuffTypes(buffTypeId)
);

INSERT INTO Buffs VALUES (1, 'AttackDamageUp', 1, 5);
INSERT INTO Buffs VALUES (2, 'DamageReduction', 2, 5);
INSERT INTO Buffs VALUES (3, 'AbilityRangeUp', 3, 2);

-- ============================================
-- Create Traps table
-- ============================================
CREATE TABLE Traps (
    trapId INTEGER PRIMARY KEY,
    trapName TEXT NOT NULL UNIQUE,
    trapCooldown REAL NOT NULL,
    trapDamage REAL DEFAULT NULL,
    trapRange REAL DEFAULT NULL,
    trapProjectileSpeed REAL DEFAULT NULL,
    trapDuration REAL DEFAULT NULL
);

-- Insert Traps with correct data
INSERT INTO Traps VALUES (1, 'Spikes', 1.5, 15, 1, NULL, NULL);
INSERT INTO Traps VALUES (2, 'Cannon', 4, 15, NULL, 50, 15);

-- ============================================
-- Verify data
-- ============================================
SELECT 'Database created successfully!' AS Status;
SELECT 'Characters count: ' || COUNT(*) FROM Characters;
SELECT 'Abilities count: ' || COUNT(*) FROM Abilities;
SELECT 'AbilityTypes count: ' || COUNT(*) FROM AbilityTypes;
SELECT 'Traps count: ' || COUNT(*) FROM Traps;
SELECT 'BuffTypes count: ' || COUNT(*) FROM BuffTypes;
SELECT 'Buffs count: ' || COUNT(*) FROM Buffs;
