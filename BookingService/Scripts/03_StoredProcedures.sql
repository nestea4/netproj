USE CinemaBookingDB;

DELIMITER //

-- ============================================
-- SP1: Створення нового бронювання
-- Вхідні: CustomerId, TotalAmount
-- Вихідні: BookingId, BookingNumber
-- ===========================================

DROP PROCEDURE IF EXISTS sp_CreateBooking//

CREATE PROCEDURE sp_CreateBooking(
    IN p_CustomerId BIGINT,
    IN p_TotalAmount DECIMAL(10,2),
    IN p_CreatedBy VARCHAR(100),
    OUT p_BookingId BIGINT,
    OUT p_BookingNumber VARCHAR(50)
)
BEGIN
    DECLARE v_BookingNumber VARCHAR(50);
    DECLARE v_Year INT;
    DECLARE v_RandomPart INT;
    
    -- Генерую унікальний номер бронювання
    SET v_Year = YEAR(NOW());
    SET v_RandomPart = FLOOR(100000 + RAND() * 900000);
    SET v_BookingNumber = CONCAT('BK-', v_Year, '-', LPAD(v_RandomPart, 6, '0'));
    
    -- Перевірка існування клієнта
    IF NOT EXISTS (SELECT 1 FROM Customer WHERE CustomerId = p_CustomerId AND IsDeleted = FALSE) THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Customer not found or deleted';
END IF;
    
    -- Вставю бронювання
INSERT INTO Booking (CustomerId, BookingNumber, TotalAmount, Status, CreatedBy)
VALUES (p_CustomerId, v_BookingNumber, p_TotalAmount, 'Pending', p_CreatedBy);

SET p_BookingId = LAST_INSERT_ID();
    SET p_BookingNumber = v_BookingNumber;
    
    -- Створюю деталі бронювання (1:1 зв'язок)
INSERT INTO BookingDetails (BookingId, CreatedBy)
VALUES (p_BookingId, p_CreatedBy);

-- Логую створення в історію
INSERT INTO BookingStatusHistory (BookingId, OldStatus, NewStatus, ChangedBy, Reason)
VALUES (p_BookingId, NULL, 'Pending', p_CreatedBy, 'Booking created');
END//

-- ============================================
-- SP2: Додавання квитка до бронювання
-- Перевірка унікальності місця
-- Оновлення TotalAmount
-- ============================================

DROP PROCEDURE IF EXISTS sp_AddTicketToBooking//

CREATE PROCEDURE sp_AddTicketToBooking(
    IN p_BookingId BIGINT,
    IN p_ShowtimeId BIGINT,
    IN p_MovieTitle VARCHAR(255),
    IN p_ShowDateTime DATETIME,
    IN p_HallName VARCHAR(100),
    IN p_SeatRow VARCHAR(10),
    IN p_SeatNumber VARCHAR(10),
    IN p_TicketPrice DECIMAL(10,2),
    IN p_TicketType VARCHAR(20),
    IN p_CreatedBy VARCHAR(100)
)
BEGIN
    DECLARE v_SeatTaken INT;
    DECLARE v_BookingStatus VARCHAR(50);
    
    -- перевірка існування бронювання
SELECT Status INTO v_BookingStatus
FROM Booking
WHERE BookingId = p_BookingId AND IsDeleted = FALSE;

IF v_BookingStatus IS NULL THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Booking not found';
END IF;
    
    -- тільки Pending та Confirmed можна додавати квитки
    IF v_BookingStatus NOT IN ('Pending', 'Confirmed') THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Cannot add tickets to paid or cancelled booking';
END IF;
    
    -- перевірка чи місце не зайняте
SELECT COUNT(*) INTO v_SeatTaken
FROM Ticket
WHERE ShowtimeId = p_ShowtimeId
  AND SeatRow = p_SeatRow
  AND SeatNumber = p_SeatNumber
  AND IsDeleted = FALSE;

IF v_SeatTaken > 0 THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Seat already taken';
END IF;
    
    -- додаю квиток
INSERT INTO Ticket (
    BookingId, ShowtimeId, MovieTitle, ShowDateTime, HallName,
    SeatRow, SeatNumber, TicketPrice, TicketType, CreatedBy
) VALUES (
             p_BookingId, p_ShowtimeId, p_MovieTitle, p_ShowDateTime, p_HallName,
             p_SeatRow, p_SeatNumber, p_TicketPrice, p_TicketType, p_CreatedBy
         );

-- оновлюємо загальну суму бронювання
UPDATE Booking
SET TotalAmount = TotalAmount + p_TicketPrice,
    UpdatedAt = NOW(),
    UpdatedBy = p_CreatedBy
WHERE BookingId = p_BookingId;
END//

-- ============================================
-- SP 3: Підтвердження бронювання
-- Зміна статусу Pending → Confirmed
-- ============================================

DROP PROCEDURE IF EXISTS sp_ConfirmBooking//

CREATE PROCEDURE sp_ConfirmBooking(
    IN p_BookingId BIGINT,
    IN p_ConfirmedBy VARCHAR(100)
)
BEGIN
    DECLARE v_CurrentStatus VARCHAR(50);
    DECLARE v_TicketCount INT;
    
    -- отримую поточний статус
SELECT Status INTO v_CurrentStatus
FROM Booking
WHERE BookingId = p_BookingId AND IsDeleted = FALSE;

IF v_CurrentStatus IS NULL THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Booking not found';
END IF;
    
    IF v_CurrentStatus != 'Pending' THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Only pending bookings can be confirmed';
END IF;
    
    -- перевірка наявності квитків
SELECT COUNT(*) INTO v_TicketCount
FROM Ticket
WHERE BookingId = p_BookingId AND IsDeleted = FALSE;

IF v_TicketCount = 0 THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Booking must have at least one ticket';
END IF;

START TRANSACTION;

-- Оновлюю статус
UPDATE Booking
SET Status = 'Confirmed',
    UpdatedAt = NOW(),
    UpdatedBy = p_ConfirmedBy
WHERE BookingId = p_BookingId;

-- Відправка email підтвердження (симуляція)
UPDATE BookingDetails
SET ConfirmationEmailSent = TRUE
WHERE BookingId = p_BookingId;

-- Логую зміну статусу
INSERT INTO BookingStatusHistory (BookingId, OldStatus, NewStatus, ChangedBy, Reason)
VALUES (p_BookingId, v_CurrentStatus, 'Confirmed', p_ConfirmedBy, 'Booking confirmed, seats reserved');

COMMIT;
END//

-- ============================================
-- SP 4: Оплата бронювання
-- Зміна статусу → Paid
-- ============================================

DROP PROCEDURE IF EXISTS sp_PayBooking//

CREATE PROCEDURE sp_PayBooking(
    IN p_BookingId BIGINT,
    IN p_PaymentMethod VARCHAR(50),
    IN p_PaidBy VARCHAR(100)
)
BEGIN
    DECLARE v_CurrentStatus VARCHAR(50);

SELECT Status INTO v_CurrentStatus
FROM Booking
WHERE BookingId = p_BookingId AND IsDeleted = FALSE;

IF v_CurrentStatus IS NULL THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Booking not found';
END IF;
    
    IF v_CurrentStatus NOT IN ('Pending', 'Confirmed') THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Booking cannot be paid';
END IF;

START TRANSACTION;

-- Оновлюю статус та спосіб оплати
UPDATE Booking
SET Status = 'Paid',
    PaymentMethod = p_PaymentMethod,
    UpdatedAt = NOW(),
    UpdatedBy = p_PaidBy
WHERE BookingId = p_BookingId;

-- Відмічаю що email відправлено
UPDATE BookingDetails
SET ConfirmationEmailSent = TRUE
WHERE BookingId = p_BookingId;

-- Логую оплату
INSERT INTO BookingStatusHistory (BookingId, OldStatus, NewStatus, ChangedBy, Reason)
VALUES (p_BookingId, v_CurrentStatus, 'Paid', p_PaidBy, CONCAT('Payment completed via ', p_PaymentMethod));

COMMIT;
END//

-- ============================================
-- SP 5: Скасування бронювання
-- Soft delete квитків
-- ============================================

DROP PROCEDURE IF EXISTS sp_CancelBooking//

CREATE PROCEDURE sp_CancelBooking(
    IN p_BookingId BIGINT,
    IN p_Reason TEXT,
    IN p_CancelledBy VARCHAR(100)
)
BEGIN
    DECLARE v_CurrentStatus VARCHAR(50);

SELECT Status INTO v_CurrentStatus
FROM Booking
WHERE BookingId = p_BookingId AND IsDeleted = FALSE;

IF v_CurrentStatus IS NULL THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Booking not found';
END IF;
    
    IF v_CurrentStatus = 'Cancelled' THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Booking already cancelled';
END IF;

START TRANSACTION;

-- Оновлюю статус бронювання
UPDATE Booking
SET Status = 'Cancelled',
    UpdatedAt = NOW(),
    UpdatedBy = p_CancelledBy
WHERE BookingId = p_BookingId;

-- Soft delete всіх квитків
UPDATE Ticket
SET IsDeleted = TRUE
WHERE BookingId = p_BookingId;

-- Логую скасування
INSERT INTO BookingStatusHistory (BookingId, OldStatus, NewStatus, ChangedBy, Reason)
VALUES (p_BookingId, v_CurrentStatus, 'Cancelled', p_CancelledBy, p_Reason);

COMMIT;
END//

-- ============================================
-- SP 6: Отримання повної інформації про бронювання
-- Повертаю тут 2 result sets: Booking + Tickets
-- ============================================

DROP PROCEDURE IF EXISTS sp_GetBookingWithTickets//

CREATE PROCEDURE sp_GetBookingWithTickets(
    IN p_BookingId BIGINT
)
BEGIN
    -- Result Set 1: Основна інформація про бронювання
SELECT
    b.BookingId,
    b.BookingNumber,
    b.BookingDate,
    b.TotalAmount,
    b.Status,
    b.PaymentMethod,
    b.CreatedAt,
    b.UpdatedAt,
    c.CustomerId,
    c.FirstName,
    c.LastName,
    c.Email,
    c.Phone,
    bd.DiscountCode,
    bd.DiscountAmount,
    bd.Notes,
    bd.ConfirmationEmailSent,
    (b.TotalAmount - COALESCE(bd.DiscountAmount, 0)) AS FinalAmount
FROM Booking b
         INNER JOIN Customer c ON b.CustomerId = c.CustomerId
         LEFT JOIN BookingDetails bd ON b.BookingId = bd.BookingId
WHERE b.BookingId = p_BookingId AND b.IsDeleted = FALSE;

-- Result Set 2: Квитки
SELECT
    t.TicketId,
    t.ShowtimeId,
    t.MovieTitle,
    t.ShowDateTime,
    t.HallName,
    t.SeatRow,
    t.SeatNumber,
    CONCAT(t.SeatRow, t.SeatNumber) AS FullSeat,
    t.TicketPrice,
    t.TicketType,
    t.CreatedAt
FROM Ticket t
WHERE t.BookingId = p_BookingId AND t.IsDeleted = FALSE
ORDER BY t.SeatRow, t.SeatNumber;
END//

DELIMITER ;

-- ============================================
-- Тестування процедур
-- ============================================

-- Тест 1: Створення бронювання
CALL sp_CreateBooking(1, 0, 'TestUser', @bookingId, @bookingNumber);
SELECT @bookingId AS NewBookingId, @bookingNumber AS BookingNumber;

-- Тест 2: Додавання квитка
CALL sp_AddTicketToBooking(
    @bookingId, 
    999, 
    'Test film', 
    '2025-12-01 20:00:00', 
    'Hall TEST', 
    'Z', 
    '99', 
    200.00, 
    'Adult', 
    'TestUser'
);

-- Тест 3: Підтвердження
CALL sp_ConfirmBooking(@bookingId, 'TestUser');

-- Тест 4: Оплата
CALL sp_PayBooking(@bookingId, 'TestCard', 'TestUser');

-- Тест 5: Перегляд
CALL sp_GetBookingWithTickets(@bookingId);

-- Тест 6: Скасування (опціонально)
-- CALL sp_CancelBooking(@bookingId, 'Test cancellation', 'TestUser');

-- Очистка тестових даних
DELETE FROM BookingStatusHistory WHERE BookingId = @bookingId;
DELETE FROM Ticket WHERE BookingId = @bookingId;
DELETE FROM BookingDetails WHERE BookingId = @bookingId;
DELETE FROM Booking WHERE BookingId = @bookingId;