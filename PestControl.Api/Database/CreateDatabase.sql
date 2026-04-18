-- =============================================
-- PestPro Database Creation Script
-- =============================================

-- Create the database
CREATE DATABASE PestControlDB;
GO

USE PestControlDB;
GO

-- =============================================
-- Table: Customers
-- =============================================
CREATE TABLE Customers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Address NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PropertyType NVARCHAR(50) NOT NULL
);

-- =============================================
-- Table: PestTypes
-- =============================================
CREATE TABLE PestTypes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Category NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    RiskLevel NVARCHAR(20) NOT NULL
);

-- =============================================
-- Table: Technicians
-- =============================================
CREATE TABLE Technicians (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Specialisation NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Available BIT NOT NULL DEFAULT 1
);

-- =============================================
-- Table: Bookings
-- =============================================
CREATE TABLE Bookings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    PestTypeId INT NOT NULL,
    TechnicianId INT NOT NULL,
    Date NVARCHAR(20) NOT NULL,
    Time NVARCHAR(10) NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    Location NVARCHAR(255) NOT NULL,
    Notes NVARCHAR(500) NOT NULL,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    FOREIGN KEY (PestTypeId) REFERENCES PestTypes(Id),
    FOREIGN KEY (TechnicianId) REFERENCES Technicians(Id)
);

-- =============================================
-- Table: Treatments
-- =============================================
CREATE TABLE Treatments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(100) NOT NULL,
    Method NVARCHAR(100) NOT NULL,
    TargetPestTypeId INT NOT NULL,
    SafetyInfo NVARCHAR(500) NOT NULL,
    FOREIGN KEY (TargetPestTypeId) REFERENCES PestTypes(Id)
);

-- =============================================
-- Table: InspectionReports
-- =============================================
CREATE TABLE InspectionReports (
    Id INT PRIMARY KEY IDENTITY(1,1),
    BookingId INT NOT NULL,
    Findings NVARCHAR(1000) NOT NULL,
    Recommendations NVARCHAR(1000) NOT NULL,
    FollowUpNeeded BIT NOT NULL DEFAULT 0,
    ReportDate NVARCHAR(20) NOT NULL,
    FOREIGN KEY (BookingId) REFERENCES Bookings(Id)
);

-- =============================================
-- Sample Data: Customers
-- =============================================
INSERT INTO Customers (Name, Address, Phone, Email, PropertyType) VALUES
('John Smith', '12 Oak Lane, London, E1 6AN', '07700900001', 'john.smith@email.com', 'Residential'),
('Sarah Johnson', '45 High Street, Manchester, M1 1AD', '07700900002', 'sarah.j@email.com', 'Commercial'),
('Premier Restaurant Ltd', '88 Victoria Road, Birmingham, B1 1BB', '07700900003', 'info@premierrest.com', 'Commercial'),
('David Williams', '3 Park Avenue, Leeds, LS1 1UR', '07700900004', 'd.williams@email.com', 'Residential'),
('Emma Brown', '27 Church Street, Bristol, BS1 1HT', '07700900005', 'emma.b@email.com', 'Residential'),
('City Hotel Group', '100 Queen Street, Edinburgh, EH2 1JE', '07700900006', 'ops@cityhotel.com', 'Commercial'),
('Michael Taylor', '9 Mill Lane, Cardiff, CF10 1FL', '07700900007', 'm.taylor@email.com', 'Residential'),
('Greenfield School', '55 Academy Road, Liverpool, L1 1JQ', '07700900008', 'admin@greenfield.edu', 'Commercial'),
('Lisa Anderson', '71 Elm Close, Nottingham, NG1 1AB', '07700900009', 'lisa.a@email.com', 'Residential'),
('Thames Warehouse Co', '200 Dock Road, London, E14 9TS', '07700900010', 'contact@thameswarehouse.com', 'Industrial');

-- =============================================
-- Sample Data: PestTypes
-- =============================================
INSERT INTO PestTypes (Name, Category, Description, RiskLevel) VALUES
('Brown Rat', 'Rodents', 'Common rat found in urban areas. Can cause structural damage and spread disease.', 'High'),
('House Mouse', 'Rodents', 'Small rodent that contaminates food supplies and causes wiring damage.', 'Medium'),
('German Cockroach', 'Insects', 'Fast-breeding insect found in kitchens and bathrooms. Carries bacteria.', 'High'),
('Bed Bug', 'Insects', 'Parasitic insect that feeds on human blood. Causes itchy bites and allergic reactions.', 'Medium'),
('Common Wasp', 'Insects', 'Aggressive when threatened. Nests in roofs, walls and garden structures.', 'Medium'),
('Feral Pigeon', 'Birds', 'Urban bird that damages buildings with droppings and carries diseases.', 'Low'),
('Grey Squirrel', 'Wildlife', 'Can cause significant damage to roof spaces, wiring and insulation.', 'Medium'),
('Fox', 'Wildlife', 'Urban fox that scavenges bins and can carry mange and other diseases.', 'Low'),
('Clothes Moth', 'Insects', 'Larvae damage natural fabrics, carpets and upholstery.', 'Low'),
('Pharaoh Ant', 'Insects', 'Tropical ant species common in heated buildings. Difficult to eradicate.', 'High'),
('Black Garden Ant', 'Insects', 'Common ant that nests outdoors but enters buildings seeking food.', 'Low'),
('Carpet Beetle', 'Insects', 'Larvae feed on natural fibres causing damage to carpets and clothing.', 'Low');

-- =============================================
-- Sample Data: Technicians
-- =============================================
INSERT INTO Technicians (Name, Specialisation, Phone, Email, Available) VALUES
('James Wilson', 'Rodents', '07700800001', 'j.wilson@pestpro.com', 1),
('Rachel Green', 'Insects', '07700800002', 'r.green@pestpro.com', 1),
('Tom Baker', 'Wildlife', '07700800003', 't.baker@pestpro.com', 1),
('Sophie Clark', 'Birds', '07700800004', 's.clark@pestpro.com', 0);

-- =============================================
-- Sample Data: Bookings
-- =============================================
INSERT INTO Bookings (CustomerId, PestTypeId, TechnicianId, Date, Time, Status, Location, Notes) VALUES
(1, 1, 1, '2026-03-25', '09:00', 'Confirmed', '12 Oak Lane, London, E1 6AN', 'Rat sighting in garden shed'),
(2, 3, 2, '2026-03-25', '11:00', 'In Progress', '45 High Street, Manchester, M1 1AD', 'Cockroach infestation in kitchen area'),
(3, 1, 1, '2026-03-26', '10:00', 'Pending', '88 Victoria Road, Birmingham, B1 1BB', 'Rat droppings found in storage room'),
(4, 4, 3, '2026-03-26', '14:00', 'Confirmed', '3 Park Avenue, Leeds, LS1 1UR', 'Bed bug report in bedroom'),
(5, 5, 2, '2026-03-27', '09:30', 'Pending', '27 Church Street, Bristol, BS1 1HT', 'Wasp nest under roof eaves'),
(6, 3, 4, '2026-03-27', '11:00', 'Confirmed', '100 Queen Street, Edinburgh, EH2 1JE', 'Routine cockroach inspection for hotel'),
(7, 7, 3, '2026-03-28', '10:00', 'Pending', '9 Mill Lane, Cardiff, CF10 1FL', 'Squirrel damage in loft space'),
(8, 6, 4, '2026-03-28', '13:00', 'Confirmed', '55 Academy Road, Liverpool, L1 1JQ', 'Pigeon nesting on school roof'),
(9, 2, 1, '2026-03-20', '09:00', 'Completed', '71 Elm Close, Nottingham, NG1 1AB', 'Mouse droppings in kitchen'),
(10, 1, 2, '2026-03-21', '14:00', 'Completed', '200 Dock Road, London, E14 9TS', 'Rat activity in warehouse loading bay'),
(1, 2, 1, '2026-03-18', '10:00', 'Completed', '12 Oak Lane, London, E1 6AN', 'Follow up mouse treatment'),
(3, 10, 2, '2026-03-29', '09:00', 'Pending', '88 Victoria Road, Birmingham, B1 1BB', 'Pharaoh ant sighting in restaurant kitchen');

-- =============================================
-- Sample Data: Treatments
-- =============================================
INSERT INTO Treatments (ProductName, Method, TargetPestTypeId, SafetyInfo) VALUES
('RodentBlock Pro', 'Bait Station', 1, 'Keep away from children and pets. Use tamper-resistant bait stations only. Wash hands after handling.'),
('MouseGuard Snap', 'Mechanical Trap', 2, 'Set traps along walls away from foot traffic. Check traps daily. Dispose of caught rodents hygienically.'),
('InsectaClear Gel', 'Gel Bait Application', 3, 'Apply in small dots in cracks and crevices. Avoid food preparation surfaces. Ventilate area after application.'),
('BedBugHeat Treatment', 'Thermal Remediation', 4, 'Room temperature raised to 56C for sustained period. Remove heat-sensitive items. Area must be vacated during treatment.'),
('WaspNest Powder', 'Insecticidal Dust', 5, 'Apply at dusk when wasps are less active. Wear protective equipment. Keep people away for 24 hours.'),
('PigeonNet System', 'Bird Netting', 6, 'Professional installation required. Stainless steel fixings. UV-stabilised polyethylene netting. Annual inspection recommended.'),
('SquirrelCage Trap', 'Live Capture Trap', 7, 'Check traps twice daily minimum. Bait with peanut butter. Legal requirements apply to grey squirrel release.'),
('FoxDeter Spray', 'Scent Deterrent', 8, 'Apply around perimeter weekly. Non-toxic formula. Reapply after heavy rain. Safe for garden use.'),
('MothCedar Blocks', 'Natural Repellent', 9, 'Place in wardrobes and drawers. Replace every 6 months. Sand surface to refresh scent. Non-toxic.'),
('AntBait Station', 'Bait Station', 10, 'Place near ant trails. Do not disturb stations. Allow 2-3 weeks for colony elimination. Replace monthly.');

-- =============================================
-- Sample Data: InspectionReports
-- =============================================
INSERT INTO InspectionReports (BookingId, Findings, Recommendations, FollowUpNeeded, ReportDate) VALUES
(9, 'Mouse droppings found behind kitchen units and under sink. Entry point identified at pipe gap in external wall. No live mice observed during visit.', 'Seal pipe gap with wire wool and cement. Install bait stations in kitchen and utility room. Follow up in 2 weeks.', 1, '2026-03-20'),
(10, 'Active rat burrow found near loading bay drainage. Gnaw marks on stored pallets. Droppings consistent with brown rat activity over several weeks.', 'Deploy tamper-resistant bait stations at 5 locations. Repair damaged drain cover. Remove harbourage around perimeter. Monthly monitoring contract recommended.', 1, '2026-03-21'),
(11, 'Follow-up visit. No new mouse activity detected. Bait stations undisturbed. Entry point has been sealed by property owner as recommended.', 'Remove bait stations after next clear visit. No further action required if activity remains absent.', 0, '2026-03-18');
