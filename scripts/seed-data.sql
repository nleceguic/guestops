-- ============================================================
-- GuestOps — Seed Data
-- ============================================================

USE ABAPpartmentDB;
GO

-- ============================================================
-- 1. USERS
-- Passwords: todos usan "Admin1234!" (hash BCrypt generado)
-- ============================================================

INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Phone, Role, Language, IsActive, CreatedAt) VALUES
('admin@abartment.com',    '$2a$11$KqBSWUzpqBOjFKMVvZdXROXgFkYQJDKIBSBLoZVRfQpLBHGE1RL3a', 'Carlos',    'Mendoza',   '+34 600 111 001', 'Admin',    'es', 1, GETUTCDATE()),
('operario1@abartment.com','$2a$11$KqBSWUzpqBOjFKMVvZdXROXgFkYQJDKIBSBLoZVRfQpLBHGE1RL3a', 'María',     'García',    '+34 600 111 002', 'Operator', 'es', 1, GETUTCDATE()),
('operario2@abartment.com','$2a$11$KqBSWUzpqBOjFKMVvZdXROXgFkYQJDKIBSBLoZVRfQpLBHGE1RL3a', 'Jordi',     'Puig',      '+34 600 111 003', 'Operator', 'ca', 1, GETUTCDATE()),
('owner1@abartment.com',   '$2a$11$KqBSWUzpqBOjFKMVvZdXROXgFkYQJDKIBSBLoZVRfQpLBHGE1RL3a', 'Isabel',    'Romero',    '+34 600 111 004', 'Owner',    'es', 1, GETUTCDATE()),
('owner2@abartment.com',   '$2a$11$KqBSWUzpqBOjFKMVvZdXROXgFkYQJDKIBSBLoZVRfQpLBHGE1RL3a', 'Marc',      'Vilaró',    '+34 600 111 005', 'Owner',    'ca', 1, GETUTCDATE()),
('guest1@test.com',        '$2a$11$KqBSWUzpqBOjFKMVvZdXROXgFkYQJDKIBSBLoZVRfQpLBHGE1RL3a', 'John',      'Smith',     '+44 7700 900001', 'Guest',    'en', 1, GETUTCDATE()),
('guest2@test.com',        '$2a$11$KqBSWUzpqBOjFKMVvZdXROXgFkYQJDKIBSBLoZVRfQpLBHGE1RL3a', 'Sophie',    'Dupont',    '+33 6 00 00 00 01','Guest',   'fr', 1, GETUTCDATE()),
('guest3@test.com',        '$2a$11$KqBSWUzpqBOjFKMVvZdXROXgFkYQJDKIBSBLoZVRfQpLBHGE1RL3a', 'Hans',      'Mueller',   '+49 170 0000001', 'Guest',    'de', 1, GETUTCDATE()),
('guest4@test.com',        '$2a$11$KqBSWUzpqBOjFKMVvZdXROXgFkYQJDKIBSBLoZVRfQpLBHGE1RL3a', 'Laura',     'Martínez',  '+34 600 222 001', 'Guest',    'es', 1, GETUTCDATE()),
('guest5@test.com',        '$2a$11$KqBSWUzpqBOjFKMVvZdXROXgFkYQJDKIBSBLoZVRfQpLBHGE1RL3a', 'Emily',     'Johnson',   '+1 555 000 0001', 'Guest',    'en', 1, GETUTCDATE());
GO

-- ============================================================
-- 2. APARTMENTS
-- ============================================================
INSERT INTO Apartments (OwnerId, InternalCode, Name, AddressLine, District, Bedrooms, MaxGuests, BaseNightlyRate, FloorArea, Latitude, Longitude, SmartLockCode, Status, CreatedAt) VALUES
(4, 'BCN-001', 'Ático Gràcia',         'Carrer de Verdi 28, 4º 1ª',            'Gràcia',         2, 4, 120.00, 75.0,  41.4036, 2.1577, 'SL-1001', 'Active', GETUTCDATE()),
(4, 'BCN-002', 'Loft Eixample',        'Carrer de Balmes 45, 2º 2ª',           'Eixample',       1, 2, 95.00,  50.0,  41.3900, 2.1540, 'SL-1002', 'Active', GETUTCDATE()),
(4, 'BCN-003', 'Piso Born',            'Carrer del Rec 12, 1º 1ª',             'El Born',        2, 5, 140.00, 80.0,  41.3851, 2.1826, 'SL-1003', 'Active', GETUTCDATE()),
(5, 'BCN-004', 'Estudio Barceloneta',  'Passeig Joan de Borbó 34, 3º 2ª',      'Barceloneta',    1, 2, 85.00,  35.0,  41.3794, 2.1872, 'SL-1004', 'Active', GETUTCDATE()),
(5, 'BCN-005', 'Dúplex Sarrià',        'Carrer de Calatrava 18, Baixos',        'Sarrià',         3, 6, 180.00, 120.0, 41.4053, 2.1195, 'SL-1005', 'Active', GETUTCDATE()),
(5, 'BCN-006', 'Piso Poblenou',        'Carrer de Pallars 89, 2º 1ª',           'Poblenou',       2, 4, 110.00, 70.0,  41.4011, 2.1990, 'SL-1006', 'Active', GETUTCDATE()),
(4, 'BCN-007', 'Apartamento Gótico',   'Carrer de la Boqueria 7, 3º',           'Barri Gòtic',    1, 3, 130.00, 55.0,  41.3808, 2.1734, NULL,      'Active', GETUTCDATE()),
(4, 'BCN-008', 'Suite Diagonal',       'Avinguda Diagonal 234, 5º 3ª',          'Les Corts',      2, 4, 155.00, 90.0,  41.3924, 2.1317, 'SL-1008', 'Active', GETUTCDATE()),
(5, 'BCN-009', 'Bajo Carmel',          'Carrer del Carmel 45, Baixos',          'El Carmel',      2, 4, 100.00, 80.0,  41.4180, 2.1598, 'SL-1009', 'Active', GETUTCDATE()),
(5, 'BCN-010', 'Piso Sant Gervasi',    'Carrer de Santaló 67, 1º 2ª',           'Sant Gervasi',   2, 4, 135.00, 85.0,  41.4006, 2.1394, 'SL-1010', 'UnderMaintenance', GETUTCDATE());
GO

-- ============================================================
-- 3. RESERVATIONS
-- ============================================================
INSERT INTO Reservations (ApartmentId, GuestId, CheckInDate, CheckOutDate, NumGuests, TotalAmount, Status, Channel, CheckInMethod, SpecialRequests, CreatedAt) VALUES
(1,  6, '2026-03-10', '2026-03-17', 2, 840.00,  'CheckedIn',  'Airbnb',  'SmartLock',   'Late check-in around 11pm', GETUTCDATE()),
(3,  7, '2026-03-12', '2026-03-16', 4, 560.00,  'CheckedIn',  'Booking', 'SmartLock',   'Cuna para bebé si posible', GETUTCDATE()),
(5,  8, '2026-03-13', '2026-03-20', 5, 1260.00, 'CheckedIn',  'Direct',  'SmartLock',   NULL, GETUTCDATE()),
(1,  9, '2026-03-18', '2026-03-22', 2, 480.00,  'Confirmed',  'Airbnb',  'SmartLock',   NULL, GETUTCDATE()),
(2,  10,'2026-03-15', '2026-03-19', 1, 380.00,  'Confirmed',  'Direct',  'SmartLock',   'Piso tranquilo si es posible', GETUTCDATE()),
(4,  6, '2026-03-20', '2026-03-25', 2, 425.00,  'Confirmed',  'Booking', 'SmartLock',   NULL, GETUTCDATE()),
(6,  7, '2026-03-22', '2026-03-28', 3, 660.00,  'Confirmed',  'Airbnb',  'SmartLock',   NULL, GETUTCDATE()),
(8,  8, '2026-03-25', '2026-03-30', 2, 775.00,  'Confirmed',  'Direct',  'SmartLock',   'Flores de bienvenida', GETUTCDATE()),
(9,  9, '2026-04-01', '2026-04-05', 3, 400.00,  'Confirmed',  'Booking', 'SmartLock',   NULL, GETUTCDATE()),
(7,  10,'2026-04-03', '2026-04-08', 2, 650.00,  'Confirmed',  'Direct',  'KeyBox',      NULL, GETUTCDATE()),
(2,  6, '2026-03-01', '2026-03-05', 1, 380.00,  'CheckedOut', 'Airbnb',  'SmartLock',   NULL, DATEADD(day,-15,GETUTCDATE())),
(4,  7, '2026-03-02', '2026-03-07', 2, 425.00,  'CheckedOut', 'Booking', 'SmartLock',   NULL, DATEADD(day,-14,GETUTCDATE())),
(6,  8, '2026-03-05', '2026-03-10', 3, 660.00,  'CheckedOut', 'Direct',  'SmartLock',   NULL, DATEADD(day,-12,GETUTCDATE())),
(3,  10,'2026-03-15', '2026-03-20', 2, 700.00,  'Cancelled',  'Airbnb',  'SmartLock',   NULL, DATEADD(day,-10,GETUTCDATE())),
(5,  9, '2026-03-20', '2026-03-25', 4, 900.00,  'Cancelled',  'Booking', 'SmartLock',   NULL, DATEADD(day,-8, GETUTCDATE()));

-- ============================================================
-- 4. PAYMENTS
-- ============================================================
INSERT INTO Payments (ReservationId, Amount, Type, Method, Status, TransactionRef, PaidAt, CreatedAt) VALUES
(1, 252.00, 'Deposit', 'Card',         'Completed', 'TXN-AIR-001-DEP', DATEADD(day,-10,GETUTCDATE()), DATEADD(day,-10,GETUTCDATE())),
(1, 588.00, 'Balance', 'BankTransfer', 'Completed', 'TXN-AIR-001-BAL', DATEADD(day,-3, GETUTCDATE()), DATEADD(day,-3, GETUTCDATE())),
(2, 168.00, 'Deposit', 'Card',         'Completed', 'TXN-BOO-002-DEP', DATEADD(day,-8,GETUTCDATE()),  DATEADD(day,-8,GETUTCDATE())),
(2, 392.00, 'Balance', 'BankTransfer', 'Pending',    NULL,              NULL,                          DATEADD(day,-1,GETUTCDATE())),
(3, 1260.00,'Deposit', 'Cash',         'Completed', NULL,              GETUTCDATE(),                  GETUTCDATE()),
(4, 144.00, 'Deposit', 'Stripe',       'Completed', 'TXN-STR-004-DEP', DATEADD(day,-5,GETUTCDATE()), DATEADD(day,-5,GETUTCDATE())),
(11, 380.00, 'Deposit', 'Card',         'Completed', 'TXN-AIR-011',     DATEADD(day,-18,GETUTCDATE()),DATEADD(day,-18,GETUTCDATE())),
(11, -50.00, 'Refund',  'Card',         'Refunded',  'Reembolso de pago #7: incidencia WiFi', DATEADD(day,-12,GETUTCDATE()),DATEADD(day,-12,GETUTCDATE()));
GO

-- ============================================================
-- 5. INCIDENTS
-- ============================================================
INSERT INTO Incidents (ApartmentId, ReservationId, ReportedById, AssignedToId, Category, Priority, Title, Description, Status, ZendeskTicketId, ResolvedAt, CreatedAt) VALUES
(1, 1,  6, 2, 'Maintenance', 'Critical', 'Calefacción no funciona',   'El huésped reporta que la calefacción no enciende. Temperatura interior 14°C.',  'Open',       NULL,       NULL,                          DATEADD(hour,-2,GETUTCDATE())),
(3, 2,  7, 3, 'Maintenance', 'High',     'Fuga de agua en baño',      'Fuga bajo el lavabo del baño principal. Agua acumulada en el suelo.',             'InProgress', 'ZD-00041', NULL,                          DATEADD(hour,-5,GETUTCDATE())),
(5, 3,  8, 2, 'Complaint',   'High',     'Ruido excesivo vecinos',    'Vecinos del piso superior hacen ruido constantemente desde las 23h.',             'Open',       NULL,       NULL,                          DATEADD(hour,-1,GETUTCDATE())),
(6, NULL,1, 3, 'Maintenance', 'Medium',  'Lavadora no centrifuga',    'La lavadora del apartamento no completa el ciclo de centrifugado.',               'Open',       NULL,       NULL,                          DATEADD(hour,-8,GETUTCDATE())),
(2, NULL,1, 2, 'Cleaning',   'Low',      'Manchas en sofá',           'El sofá del salón tiene manchas que no se quitaron en la última limpieza.',       'Open',       NULL,       NULL,                          DATEADD(hour,-12,GETUTCDATE())),
(4, NULL,1, 3, 'Maintenance', 'Medium',  'Bombilla fundida dormitorio','Bombilla del dormitorio principal fundida. Necesita reemplazo.',                  'InProgress', NULL,       NULL,                          DATEADD(day,-1,GETUTCDATE())),
(7, NULL,1, 2, 'Maintenance', 'Medium',  'Persiana rota',             'La persiana del salón no sube correctamente.',                                    'InProgress', 'ZD-00039', NULL,                          DATEADD(day,-1,GETUTCDATE())),
(1, NULL,1, 2, 'Maintenance', 'High',    'WiFi sin conexión',         'El router del apartamento no funcionaba.',                                        'Resolved',   'ZD-00035', DATEADD(day,-2,GETUTCDATE()),  DATEADD(day,-3,GETUTCDATE())),
(3, NULL,1, 3, 'Cleaning',   'Low',      'Toallas insuficientes',     'El huésped solicitó toallas adicionales.',                                        'Resolved',   NULL,       DATEADD(day,-1,GETUTCDATE()),  DATEADD(day,-2,GETUTCDATE())),
(8, NULL,1, 2, 'Maintenance', 'Medium',  'Aire acondicionado pitando','El AC emite un pitido constante al arrancar.',                                    'Closed',     'ZD-00030', DATEADD(day,-5,GETUTCDATE()),  DATEADD(day,-7,GETUTCDATE()));
GO

-- ============================================================
-- 6. CLEANING SCHEDULES
-- ============================================================
INSERT INTO CleaningSchedules (ApartmentId, ReservationId, AssignedToId, ScheduledDate, ScheduledTime, Type, Status, Notes, CompletedAt) VALUES
(1, 1,    2, CAST(GETUTCDATE() AS DATE), '10:00', 'Midstay',     'Scheduled',  NULL, NULL),
(3, 2,    3, CAST(GETUTCDATE() AS DATE), '11:00', 'Midstay',     'InProgress', NULL, NULL),
(5, 3,    2, CAST(GETUTCDATE() AS DATE), '12:00', 'Midstay',     'Scheduled',  'Familia con bebé, usar productos sin fragancia', NULL),
(2, NULL, 3, CAST(GETUTCDATE() AS DATE), '09:00', 'Deep',        'Done',       NULL, DATEADD(hour,-1,GETUTCDATE())),
(9, 9,    2, CAST(GETUTCDATE() AS DATE), '15:00', 'Checkout',    'Scheduled',  NULL, NULL),
(1, 4,    2, CAST(DATEADD(day,1,GETUTCDATE()) AS DATE), '11:00', 'Checkout',  'Scheduled', NULL, NULL),
(2, 5,    3, CAST(DATEADD(day,1,GETUTCDATE()) AS DATE), '10:00', 'Checkout',  'Scheduled', NULL, NULL),
(6, 7,    2, CAST(DATEADD(day,1,GETUTCDATE()) AS DATE), '12:00', 'Midstay',   'Scheduled', NULL, NULL),
(4, 6,    3, CAST(DATEADD(day,2,GETUTCDATE()) AS DATE), '11:00', 'Checkout',  'Scheduled', NULL, NULL),
(8, 8,    2, CAST(DATEADD(day,2,GETUTCDATE()) AS DATE), '10:00', 'Midstay',   'Scheduled', NULL, NULL),
(3, NULL, 3, CAST(DATEADD(day,3,GETUTCDATE()) AS DATE), '09:00', 'Deep',      'Scheduled', 'Limpieza profunda pre-temporada', NULL),
(7, 10,   2, CAST(DATEADD(day,3,GETUTCDATE()) AS DATE), '11:00', 'Checkout',  'Scheduled', NULL, NULL),
(2, 11,   2, CAST(DATEADD(day,-4,GETUTCDATE()) AS DATE), '11:00', 'Checkout', 'Done', NULL, DATEADD(day,-4,GETUTCDATE())),
(4, 12,   3, CAST(DATEADD(day,-3,GETUTCDATE()) AS DATE), '11:00', 'Checkout', 'Done', NULL, DATEADD(day,-3,GETUTCDATE())),
(6, 13,   2, CAST(DATEADD(day,-2,GETUTCDATE()) AS DATE), '11:00', 'Checkout', 'Done', NULL, DATEADD(day,-2,GETUTCDATE()));

-- ============================================================
-- 7. GUEST MESSAGES
-- ============================================================
INSERT INTO GuestMessages (ReservationId, GuestId, Channel, Direction, Body, IsAutoReply, AIConfidence, DetectedTopic, IncidentId, SentAt) VALUES
(1, 6, 'WhatsApp', 'Inbound',  'Hi! What time can I check in? My flight arrives at 10pm.', 0, NULL, NULL, NULL, DATEADD(day,-11,GETUTCDATE())),
(1, 6, 'WhatsApp', 'Outbound', 'Hi John, check-in at Ático Gràcia is from 3:00 PM. The apartment is at Carrer de Verdi 28, 4º 1ª. You''ll use a smart lock — the code is SL-1001. No need to come to the office. Your stay is from 03/10/2026 to 03/17/2026. Welcome!', 1, 88.00, 'checkin', NULL, DATEADD(day,-11,GETUTCDATE())),
(1, 6, 'WhatsApp', 'Inbound',  'What''s the WiFi password?', 0, NULL, NULL, NULL, DATEADD(day,-10,GETUTCDATE())),
(1, 6, 'WhatsApp', 'Outbound', 'Hi John, the WiFi network is AB_Guest_ÁticoGràcia and the password is in the welcome guide on the living room table. If you have connection issues, restart the router (black box in the living room). Anything else?', 1, 97.00, 'wifi', NULL, DATEADD(day,-10,GETUTCDATE())),
(1, 6, 'WhatsApp', 'Inbound',  'The heating doesn''t work, it''s really cold in the apartment', 0, NULL, NULL, NULL, DATEADD(hour,-2,GETUTCDATE())),
(1, 6, 'WhatsApp', 'Outbound', 'Hi John, we''re sorry for the inconvenience. We''ve logged your issue and our maintenance team will contact you within 2 hours. For emergencies, call us at +34 93 XXX XX XX (24/7).', 1, 92.00, 'incident', 1, DATEADD(hour,-2,GETUTCDATE())),
(2, 7, 'Email', 'Inbound',  'Bonjour, à quelle heure est le check-out?', 0, NULL, NULL, NULL, DATEADD(day,-2,GETUTCDATE())),
(2, 7, 'Email', 'Outbound', 'Hola Sophie, el check-out es antes de las 11:00h el día 16/03/2026. Por favor deja las llaves dentro del apartamento y cierra la puerta al salir. Si necesitas salida tardía, contáctanos con 24h de antelación.', 1, 87.00, 'checkout', NULL, DATEADD(day,-2,GETUTCDATE())),
(2, 7, 'Email', 'Inbound',  'Hay una fuga de agua en el baño, hay agua en el suelo', 0, NULL, NULL, NULL, DATEADD(hour,-5,GETUTCDATE())),
(2, 7, 'Email', 'Outbound', 'Hola Sophie, lamentamos los inconvenientes. Hemos registrado tu incidencia y nuestro equipo de mantenimiento se pondrá en contacto contigo en menos de 2 horas.', 1, 91.00, 'incident', 2, DATEADD(hour,-5,GETUTCDATE())),
(3, 8, 'WhatsApp', 'Inbound',  'Wie komme ich am besten vom Flughafen?', 0, NULL, NULL, NULL, DATEADD(day,-1,GETUTCDATE())),
(3, 8, 'WhatsApp', 'Outbound', 'Hola Hans, el apartamento en Sarrià tiene excelentes conexiones. Metro más cercano: L4 (línea amarilla) a 5 min a pie. Para el aeropuerto: Aerobús desde Pl. Catalunya (40 min, 6.75€) o taxi (~35€).', 1, 82.00, 'transport', NULL, DATEADD(day,-1,GETUTCDATE())),
(3, 8, 'WhatsApp', 'Inbound',  'Können Sie mir ein gutes Restaurant empfehlen?', 0, NULL, NULL, NULL, DATEADD(hour,-3,GETUTCDATE())),
(3, 8, 'WhatsApp', 'Outbound', 'Hola Hans, el barrio de Sarrià tiene opciones excelentes. Para tapas: prueba los bares de la zona alta del barrio. Para cena romántica: busca en Google Maps restaurantes Sarrià Barcelona. ¡Buen provecho!', 1, 79.00, 'food', NULL, DATEADD(hour,-3,GETUTCDATE())),
(2, 7, 'Email', 'Inbound',  'Tengo una pregunta sobre la política de cancelación de mi reserva', 0, NULL, NULL, NULL, DATEADD(minute,-30,GETUTCDATE()));
GO

-- ============================================================
-- Resumen de datos insertados
-- ============================================================
SELECT 'Users'            AS Tabla, COUNT(*) AS Registros FROM Users
UNION ALL
SELECT 'Apartments',       COUNT(*) FROM Apartments
UNION ALL
SELECT 'Reservations',     COUNT(*) FROM Reservations
UNION ALL
SELECT 'Payments',         COUNT(*) FROM Payments
UNION ALL
SELECT 'Incidents',        COUNT(*) FROM Incidents
UNION ALL
SELECT 'CleaningSchedules',COUNT(*) FROM CleaningSchedules
UNION ALL
SELECT 'GuestMessages',    COUNT(*) FROM GuestMessages;
GO