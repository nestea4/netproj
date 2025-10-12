-- ============================================
-- Проєкт №1: Booking Service Database
-- СУБД: MySQL 8.0+
-- Мікросервіс: Бронювання та продаж квитків
-- ============================================

CREATE DATABASE IF NOT EXISTS CinemaBookingDB
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE CinemaBookingDB;

-- ============================================
-- Таблиця 1: Customer (Клієнти)
-- Root Aggregate для контексту бронювання
-- ============================================
CREATE TABLE Customer (
                          CustomerId BIGINT AUTO_INCREMENT PRIMARY KEY,
                          FirstName VARCHAR(100) NOT NULL,
                          LastName VARCHAR(100) NOT NULL,
                          Email VARCHAR(255) NOT NULL,
                          Phone VARCHAR(20),
    -- Аудитні колонки
                          CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                          CreatedBy VARCHAR(100) DEFAULT 'System',
                          UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                          UpdatedBy VARCHAR(100) DEFAULT 'System',
                          IsDeleted BOOLEAN DEFAULT FALSE,
    -- Індекси
                          UNIQUE KEY uk_customer_email (Email),
                          INDEX idx_customer_phone (Phone),
                          INDEX idx_customer_name (LastName, FirstName)
) ENGINE=InnoDB COMMENT='Клієнти кінотеатру';

-- ============================================
-- Таблиця 2: Booking (Бронювання)
-- Зв'язок 1:N з Customer
-- ============================================
CREATE TABLE Booking (
                         BookingId BIGINT AUTO_INCREMENT PRIMARY KEY,
                         CustomerId BIGINT NOT NULL,
                         BookingNumber VARCHAR(50) NOT NULL,
                         BookingDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                         TotalAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
                         Status ENUM('Pending', 'Confirmed', 'Paid', 'Cancelled') DEFAULT 'Pending',
                         PaymentMethod VARCHAR(50),
    -- Аудитні колонки
                         CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                         CreatedBy VARCHAR(100) DEFAULT 'System',
                         UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                         UpdatedBy VARCHAR(100) DEFAULT 'System',
                         IsDeleted BOOLEAN DEFAULT FALSE,
    -- Зовнішні ключі
                         CONSTRAINT fk_booking_customer
                             FOREIGN KEY (CustomerId) REFERENCES Customer(CustomerId)
                                 ON DELETE RESTRICT ON UPDATE CASCADE,
    -- Обмеження
                         CONSTRAINT chk_booking_amount CHECK (TotalAmount >= 0),
    -- Індекси
                         UNIQUE KEY uk_booking_number (BookingNumber),
                         INDEX idx_booking_customer (CustomerId),
                         INDEX idx_booking_status (Status),
                         INDEX idx_booking_date (BookingDate DESC)
) ENGINE=InnoDB COMMENT='Бронювання квитків';

-- ============================================
-- Таблиця 3: BookingDetails (Деталі бронювання)
-- Зв'язок 1:1 з Booking
-- ============================================
CREATE TABLE BookingDetails (
                                BookingDetailsId BIGINT AUTO_INCREMENT PRIMARY KEY,
                                BookingId BIGINT NOT NULL,
                                DiscountCode VARCHAR(50),
                                DiscountAmount DECIMAL(10,2) DEFAULT 0,
                                Notes TEXT,
                                ConfirmationEmailSent BOOLEAN DEFAULT FALSE,
                                ReminderEmailSent BOOLEAN DEFAULT FALSE,
    -- Аудитні колонки
                                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                                CreatedBy VARCHAR(100) DEFAULT 'System',
    -- Зовнішні ключі (1:1)
                                CONSTRAINT fk_details_booking
                                    FOREIGN KEY (BookingId) REFERENCES Booking(BookingId)
                                        ON DELETE CASCADE ON UPDATE CASCADE,
    -- Обмеження
                                CONSTRAINT chk_discount_amount CHECK (DiscountAmount >= 0),
    -- Унікальність для 1:1
                                UNIQUE KEY uk_booking_details (BookingId)
) ENGINE=InnoDB COMMENT='Додаткові деталі бронювання (1:1 з Booking)';

-- ============================================
-- Таблиця 4: Ticket (Квитки)
-- Зв'язок M:N між Booking та Showtime
-- Дублювання даних з Catalog Service
-- ============================================
CREATE TABLE Ticket (
                        TicketId BIGINT AUTO_INCREMENT PRIMARY KEY,
                        BookingId BIGINT NOT NULL,
    -- Дані з Catalog Service (дублювання для автономності)
                        ShowtimeId BIGINT NOT NULL COMMENT 'ID сеансу з Catalog Service',
                        MovieTitle VARCHAR(255) NOT NULL,
                        ShowDateTime DATETIME NOT NULL,
                        HallName VARCHAR(100) NOT NULL,
    -- Дані квитка
                        SeatRow VARCHAR(10) NOT NULL,
                        SeatNumber VARCHAR(10) NOT NULL,
                        TicketPrice DECIMAL(10,2) NOT NULL,
                        TicketType ENUM('Adult', 'Child', 'Student', 'Senior') DEFAULT 'Adult',
    -- Аудитні колонки
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        CreatedBy VARCHAR(100) DEFAULT 'System',
                        IsDeleted BOOLEAN DEFAULT FALSE,
    -- Зовнішні ключі
                        CONSTRAINT fk_ticket_booking
                            FOREIGN KEY (BookingId) REFERENCES Booking(BookingId)
                                ON DELETE CASCADE ON UPDATE CASCADE,
    -- Обмеження
                        CONSTRAINT chk_ticket_price CHECK (TicketPrice >= 0),
    -- Унікальність місця на конкретний сеанс (M:N)
                        UNIQUE KEY uk_seat_showtime (ShowtimeId, SeatRow, SeatNumber),
    -- Індекси
                        INDEX idx_ticket_booking (BookingId),
                        INDEX idx_ticket_showtime (ShowtimeId),
                        INDEX idx_ticket_showdate (ShowDateTime)
) ENGINE=InnoDB COMMENT='Квитки (M:N між Booking та Showtime)';

-- ============================================
-- Таблиця 5: BookingStatusHistory (Історія статусів)
-- Зв'язок 1:N з Booking
-- Аудит змін статусу бронювання
-- ============================================
CREATE TABLE BookingStatusHistory (
                                      HistoryId BIGINT AUTO_INCREMENT PRIMARY KEY,
                                      BookingId BIGINT NOT NULL,
                                      OldStatus VARCHAR(50),
                                      NewStatus VARCHAR(50) NOT NULL,
                                      ChangedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                                      ChangedBy VARCHAR(100) DEFAULT 'System',
                                      Reason TEXT,
                                      IpAddress VARCHAR(45),
    -- Зовнішні ключі
                                      CONSTRAINT fk_history_booking
                                          FOREIGN KEY (BookingId) REFERENCES Booking(BookingId)
                                              ON DELETE CASCADE ON UPDATE CASCADE,
    -- Індекси
                                      INDEX idx_history_booking (BookingId),
                                      INDEX idx_history_date (ChangedAt DESC)
) ENGINE=InnoDB COMMENT='Історія змін статусів бронювань (аудит)';

-- ============================================
-- Додаткові індекси для оптимізації
-- ============================================

-- Композитний індекс для пошуку активних бронювань клієнта
CREATE INDEX idx_booking_customer_status
    ON Booking(CustomerId, Status, IsDeleted);

-- Індекс для звітності по датах
CREATE INDEX idx_booking_date_status
    ON Booking(BookingDate, Status);

-- Покриваючий індекс для швидкого підрахунку квитків
CREATE INDEX idx_ticket_booking_deleted
    ON Ticket(BookingId, IsDeleted);

-- ============================================
-- View для зручного читання
-- ============================================

CREATE OR REPLACE VIEW vw_BookingSummary AS
SELECT
    b.BookingId,
    b.BookingNumber,
    b.BookingDate,
    b.Status,
    b.TotalAmount,
    b.PaymentMethod,
    c.CustomerId,
    CONCAT(c.FirstName, ' ', c.LastName) AS CustomerName,
    c.Email AS CustomerEmail,
    c.Phone AS CustomerPhone,
    bd.DiscountCode,
    bd.DiscountAmount,
    (b.TotalAmount - COALESCE(bd.DiscountAmount, 0)) AS FinalAmount,
    COUNT(t.TicketId) AS TotalTickets,
    b.CreatedAt,
    b.UpdatedAt
FROM Booking b
         INNER JOIN Customer c ON b.CustomerId = c.CustomerId
         LEFT JOIN BookingDetails bd ON b.BookingId = bd.BookingId
         LEFT JOIN Ticket t ON b.BookingId = t.BookingId AND t.IsDeleted = FALSE
WHERE b.IsDeleted = FALSE
GROUP BY b.BookingId;

-- ============================================
-- Права доступу (приклад для окремого користувача)
-- ============================================

-- CREATE USER 'booking_service'@'localhost' IDENTIFIED BY 'SecurePassword123!';
-- GRANT SELECT, INSERT, UPDATE, DELETE ON CinemaBookingDB.* TO 'booking_service'@'localhost';
-- GRANT EXECUTE ON CinemaBookingDB.* TO 'booking_service'@'localhost';
-- FLUSH PRIVILEGES;

-- ============================================
-- Статистика по таблицях
-- ============================================

SELECT
    TABLE_NAME,
    TABLE_ROWS,
    ROUND(((DATA_LENGTH + INDEX_LENGTH) / 1024 / 1024), 2) AS 'Size (MB)'
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = 'CinemaBookingDB'
ORDER BY (DATA_LENGTH + INDEX_LENGTH) DESC;