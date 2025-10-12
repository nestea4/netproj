-- ============================================
-- Seed Data для Booking Service (English version)
-- Тестові дані для демонстрації роботи
-- ============================================

USE CinemaBookingDB;

-- ============================================
-- 1. Додавання тестових клієнтів
-- ============================================

INSERT INTO Customer (FirstName, LastName, Email, Phone, CreatedBy) VALUES
                                                                        ('Ivan', 'Petrenko', 'ivan.petrenko@email.com', '+380501234567', 'SeedScript'),
                                                                        ('Maria', 'Kovalenko', 'maria.kovalenko@email.com', '+380502345678', 'SeedScript'),
                                                                        ('Oleg', 'Shevchenko', 'oleg.shevchenko@email.com', '+380503456789', 'SeedScript'),
                                                                        ('Anna', 'Melnyk', 'anna.melnyk@email.com', '+380504567890', 'SeedScript'),
                                                                        ('Dmytro', 'Bondarenko', 'dmytro.bondarenko@email.com', '+380505678901', 'SeedScript');

-- ============================================
-- 2. Додавання тестових бронювань
-- ============================================

-- Бронювання 1: Оплачене
INSERT INTO Booking (CustomerId, BookingNumber, BookingDate, TotalAmount, Status, PaymentMethod, CreatedBy)
VALUES (1, 'BK-2025-000001', '2025-10-01 14:30:00', 275.00, 'Paid', 'CreditCard', 'SeedScript');

-- Бронювання 2: Підтверджене
INSERT INTO Booking (CustomerId, BookingNumber, BookingDate, TotalAmount, Status, CreatedBy)
VALUES (2, 'BK-2025-000002', '2025-10-05 16:45:00', 180.00, 'Confirmed', 'SeedScript');

-- Бронювання 3: В очікуванні
INSERT INTO Booking (CustomerId, BookingNumber, BookingDate, TotalAmount, Status, CreatedBy)
VALUES (3, 'BK-2025-000003', '2025-10-10 10:15:00', 320.00, 'Pending', 'SeedScript');

-- Бронювання 4: Скасоване
INSERT INTO Booking (CustomerId, BookingNumber, BookingDate, TotalAmount, Status, CreatedBy)
VALUES (4, 'BK-2025-000004', '2025-10-08 18:20:00', 150.00, 'Cancelled', 'SeedScript');

-- Бронювання 5: Оплачене (для того ж клієнта #1)
INSERT INTO Booking (CustomerId, BookingNumber, BookingDate, TotalAmount, Status, PaymentMethod, CreatedBy)
VALUES (1, 'BK-2025-000005', '2025-10-09 19:00:00', 450.00, 'Paid', 'Cash', 'SeedScript');

-- ============================================
-- 3. Додавання деталей бронювань (1:1)
-- ============================================

INSERT INTO BookingDetails (BookingId, DiscountCode, DiscountAmount, ConfirmationEmailSent, Notes, CreatedBy) VALUES
                                                                                                                  (1, 'FIRST10', 25.00, TRUE, 'First customer booking', 'SeedScript'),
                                                                                                                  (2, NULL, 0.00, TRUE, NULL, 'SeedScript'),
                                                                                                                  (3, 'STUDENT15', 48.00, FALSE, 'Student discount applied', 'SeedScript'),
                                                                                                                  (4, NULL, 0.00, FALSE, 'Cancelled by customer', 'SeedScript'),
                                                                                                                  (5, 'VIP20', 90.00, TRUE, 'VIP customer', 'SeedScript');

-- ============================================
-- 4. Додавання квитків (M:N з Showtime)
-- Дублюємо дані з Catalog Service
-- ============================================

-- Квитки для бронювання #1
INSERT INTO Ticket (BookingId, ShowtimeId, MovieTitle, ShowDateTime, HallName, SeatRow, SeatNumber, TicketPrice, TicketType, CreatedBy) VALUES
                                                                                                                                            (1, 101, 'Dune: Part Two', '2025-10-15 19:00:00', 'Hall 1', 'A', '15', 150.00, 'Adult', 'SeedScript'),
                                                                                                                                            (1, 101, 'Dune: Part Two', '2025-10-15 19:00:00', 'Hall 1', 'A', '16', 125.00, 'Student', 'SeedScript');

-- Квитки для бронювання #2
INSERT INTO Ticket (BookingId, ShowtimeId, MovieTitle, ShowDateTime, HallName, SeatRow, SeatNumber, TicketPrice, TicketType, CreatedBy) VALUES
    (2, 102, 'Oppenheimer', '2025-10-16 20:30:00', 'Hall 2', 'B', '10', 180.00, 'Adult', 'SeedScript');

-- Квитки для бронювання #3
INSERT INTO Ticket (BookingId, ShowtimeId, MovieTitle, ShowDateTime, HallName, SeatRow, SeatNumber, TicketPrice, TicketType, CreatedBy) VALUES
                                                                                                                                            (3, 103, 'Poor Things', '2025-10-17 18:00:00', 'Hall 3', 'C', '5', 160.00, 'Adult', 'SeedScript'),
                                                                                                                                            (3, 103, 'Poor Things', '2025-10-17 18:00:00', 'Hall 3', 'C', '6', 160.00, 'Adult', 'SeedScript');

-- Квитки для бронювання #4 (скасоване)
INSERT INTO Ticket (BookingId, ShowtimeId, MovieTitle, ShowDateTime, HallName, SeatRow, SeatNumber, TicketPrice, TicketType, CreatedBy, IsDeleted) VALUES
    (4, 104, 'Killers of the Flower Moon', '2025-10-18 21:00:00', 'Hall 1', 'D', '12', 150.00, 'Adult', 'SeedScript', TRUE);

-- Квитки для бронювання #5
INSERT INTO Ticket (BookingId, ShowtimeId, MovieTitle, ShowDateTime, HallName, SeatRow, SeatNumber, TicketPrice, TicketType, CreatedBy) VALUES
                                                                                                                                            (5, 105, 'Barbie', '2025-10-19 15:30:00', 'Hall 4', 'E', '8', 150.00, 'Adult', 'SeedScript'),
                                                                                                                                            (5, 105, 'Barbie', '2025-10-19 15:30:00', 'Hall 4', 'E', '9', 120.00, 'Child', 'SeedScript'),
                                                                                                                                            (5, 105, 'Barbie', '2025-10-19 15:30:00', 'Hall 4', 'E', '10', 180.00, 'Adult', 'SeedScript');

-- ============================================
-- 5. Історія статусів бронювань (аудит)
-- ============================================

-- Історія для бронювання #1
INSERT INTO BookingStatusHistory (BookingId, OldStatus, NewStatus, ChangedBy, Reason, ChangedAt) VALUES
                                                                                                     (1, NULL, 'Pending', 'System', 'Booking created', '2025-10-01 14:30:00'),
                                                                                                     (1, 'Pending', 'Confirmed', 'System', 'Seats reserved', '2025-10-01 14:31:00'),
                                                                                                     (1, 'Confirmed', 'Paid', 'Customer', 'Payment completed via credit card', '2025-10-01 14:35:00');

-- Історія для бронювання #2
INSERT INTO BookingStatusHistory (BookingId, OldStatus, NewStatus, ChangedBy, Reason, ChangedAt) VALUES
                                                                                                     (2, NULL, 'Pending', 'System', 'Booking created', '2025-10-05 16:45:00'),
                                                                                                     (2, 'Pending', 'Confirmed', 'System', 'Seats reserved', '2025-10-05 16:46:00');

-- Історія для бронювання #3
INSERT INTO BookingStatusHistory (BookingId, OldStatus, NewStatus, ChangedBy, Reason, ChangedAt) VALUES
    (3, NULL, 'Pending', 'System', 'Booking created', '2025-10-10 10:15:00');

-- Історія для бронювання #4
INSERT INTO BookingStatusHistory (BookingId, OldStatus, NewStatus, ChangedBy, Reason, ChangedAt) VALUES
                                                                                                     (4, NULL, 'Pending', 'System', 'Booking created', '2025-10-08 18:20:00'),
                                                                                                     (4, 'Pending', 'Confirmed', 'System', 'Seats reserved', '2025-10-08 18:21:00'),
                                                                                                     (4, 'Confirmed', 'Cancelled', 'Customer', 'Customer changed mind', '2025-10-08 20:00:00');

-- Історія для бронювання #5
INSERT INTO BookingStatusHistory (BookingId, OldStatus, NewStatus, ChangedBy, Reason, ChangedAt) VALUES
                                                                                                     (5, NULL, 'Pending', 'System', 'Booking created', '2025-10-09 19:00:00'),
                                                                                                     (5, 'Pending', 'Confirmed', 'System', 'Seats reserved', '2025-10-09 19:01:00'),
                                                                                                     (5, 'Confirmed', 'Paid', 'Customer', 'Payment completed in cash', '2025-10-09 19:05:00');

-- ============================================
-- Перевірка доданих даних
-- ============================================

SELECT 'Customers' AS TableName, COUNT(*) AS RecordCount FROM Customer
UNION ALL
SELECT 'Bookings', COUNT(*) FROM Booking
UNION ALL
SELECT 'BookingDetails', COUNT(*) FROM BookingDetails
UNION ALL
SELECT 'Tickets', COUNT(*) FROM Ticket WHERE IsDeleted = FALSE
UNION ALL
SELECT 'StatusHistory', COUNT(*) FROM BookingStatusHistory;

-- Перегляд через View
SELECT * FROM vw_BookingSummary ORDER BY BookingDate DESC;