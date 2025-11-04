using System.Data;
using BookingService.Dal.Interfaces;
using BookingService.Domain.Models;
using MySqlConnector;

namespace BookingService.Dal.Repositories;

/// <summary>
/// Репозиторій для Customer на чистому ADO.NET
///тута є робота з підключеннями, командами, параметрами та DataReader
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly DbConnectionFactory _connectionFactory;
    private MySqlConnection? _sharedConnection;
    private MySqlTransaction? _transaction;

    public CustomerRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <summary>
    /// встановлює спільне підключення для UoW
    /// </summary>
    public void SetSharedConnection(MySqlConnection connection, MySqlTransaction? transaction = null)
    {
        _sharedConnection = connection;
        _transaction = transaction;
    }

    private MySqlConnection GetConnection()
    {
        return _sharedConnection ?? _connectionFactory.CreateConnection();
    }

    private bool ShouldDisposeConnection()
    {
        return _sharedConnection == null;
    }

    public async Task<Customer?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var connection = GetConnection();
        var shouldDispose = ShouldDisposeConnection();

        try
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.Transaction = _transaction;
            command.CommandText = @"
                SELECT CustomerId, FirstName, LastName, Email, Phone, 
                       CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
                FROM Customer 
                WHERE CustomerId = @CustomerId AND IsDeleted = FALSE";

            command.Parameters.Add(new MySqlParameter("@CustomerId", MySqlDbType.Int64) { Value = id });

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            
            if (await reader.ReadAsync(cancellationToken))
            {
                return MapCustomerFromReader(reader);
            }

            return null;
        }
        finally
        {
            if (shouldDispose)
            {
                await connection.DisposeAsync();
            }
        }
    }

    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var connection = GetConnection();
        var shouldDispose = ShouldDisposeConnection();

        try
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.Transaction = _transaction;
            command.CommandText = @"
                SELECT CustomerId, FirstName, LastName, Email, Phone, 
                       CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
                FROM Customer 
                WHERE IsDeleted = FALSE
                ORDER BY CustomerId DESC
                LIMIT 1000";

            var customers = new List<Customer>();
            
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                customers.Add(MapCustomerFromReader(reader));
            }

            return customers;
        }
        finally
        {
            if (shouldDispose)
            {
                await connection.DisposeAsync();
            }
        }
    }

    public async Task<long> CreateAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var connection = GetConnection();
        var shouldDispose = ShouldDisposeConnection();

        try
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.Transaction = _transaction;
            command.CommandText = @"
                INSERT INTO Customer (FirstName, LastName, Email, Phone, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
                VALUES (@FirstName, @LastName, @Email, @Phone, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy, FALSE);
                SELECT LAST_INSERT_ID();";

            command.Parameters.Add(new MySqlParameter("@FirstName", MySqlDbType.VarChar, 100) { Value = entity.FirstName });
            command.Parameters.Add(new MySqlParameter("@LastName", MySqlDbType.VarChar, 100) { Value = entity.LastName });
            command.Parameters.Add(new MySqlParameter("@Email", MySqlDbType.VarChar, 255) { Value = entity.Email });
            command.Parameters.Add(new MySqlParameter("@Phone", MySqlDbType.VarChar, 20) { Value = (object?)entity.Phone ?? DBNull.Value });
            command.Parameters.Add(new MySqlParameter("@CreatedAt", MySqlDbType.DateTime) { Value = DateTime.Now });
            command.Parameters.Add(new MySqlParameter("@CreatedBy", MySqlDbType.VarChar, 100) { Value = entity.CreatedBy });
            command.Parameters.Add(new MySqlParameter("@UpdatedAt", MySqlDbType.DateTime) { Value = DateTime.Now });
            command.Parameters.Add(new MySqlParameter("@UpdatedBy", MySqlDbType.VarChar, 100) { Value = entity.UpdatedBy });

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt64(result);
        }
        finally
        {
            if (shouldDispose)
            {
                await connection.DisposeAsync();
            }
        }
    }

    public async Task<bool> UpdateAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var connection = GetConnection();
        var shouldDispose = ShouldDisposeConnection();

        try
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.Transaction = _transaction;
            command.CommandText = @"
                UPDATE Customer 
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email,
                    Phone = @Phone,
                    UpdatedAt = @UpdatedAt,
                    UpdatedBy = @UpdatedBy
                WHERE CustomerId = @CustomerId AND IsDeleted = FALSE";

            command.Parameters.Add(new MySqlParameter("@CustomerId", MySqlDbType.Int64) { Value = entity.CustomerId });
            command.Parameters.Add(new MySqlParameter("@FirstName", MySqlDbType.VarChar, 100) { Value = entity.FirstName });
            command.Parameters.Add(new MySqlParameter("@LastName", MySqlDbType.VarChar, 100) { Value = entity.LastName });
            command.Parameters.Add(new MySqlParameter("@Email", MySqlDbType.VarChar, 255) { Value = entity.Email });
            command.Parameters.Add(new MySqlParameter("@Phone", MySqlDbType.VarChar, 20) { Value = (object?)entity.Phone ?? DBNull.Value });
            command.Parameters.Add(new MySqlParameter("@UpdatedAt", MySqlDbType.DateTime) { Value = DateTime.Now });
            command.Parameters.Add(new MySqlParameter("@UpdatedBy", MySqlDbType.VarChar, 100) { Value = entity.UpdatedBy });

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }
        finally
        {
            if (shouldDispose)
            {
                await connection.DisposeAsync();
            }
        }
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var connection = GetConnection();
        var shouldDispose = ShouldDisposeConnection();

        try
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.Transaction = _transaction;
            command.CommandText = "DELETE FROM Customer WHERE CustomerId = @CustomerId";
            command.Parameters.Add(new MySqlParameter("@CustomerId", MySqlDbType.Int64) { Value = id });

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }
        finally
        {
            if (shouldDispose)
            {
                await connection.DisposeAsync();
            }
        }
    }

    public async Task<bool> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var connection = GetConnection();
        var shouldDispose = ShouldDisposeConnection();

        try
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.Transaction = _transaction;
            command.CommandText = @"
                UPDATE Customer 
                SET IsDeleted = TRUE, 
                    UpdatedAt = @UpdatedAt,
                    UpdatedBy = @UpdatedBy
                WHERE CustomerId = @CustomerId";

            command.Parameters.Add(new MySqlParameter("@CustomerId", MySqlDbType.Int64) { Value = id });
            command.Parameters.Add(new MySqlParameter("@UpdatedAt", MySqlDbType.DateTime) { Value = DateTime.Now });
            command.Parameters.Add(new MySqlParameter("@UpdatedBy", MySqlDbType.VarChar, 100) { Value = "System" });

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }
        finally
        {
            if (shouldDispose)
            {
                await connection.DisposeAsync();
            }
        }
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        var connection = GetConnection();
        var shouldDispose = ShouldDisposeConnection();

        try
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.Transaction = _transaction;
            command.CommandText = @"
                SELECT CustomerId, FirstName, LastName, Email, Phone, 
                       CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
                FROM Customer 
                WHERE Email = @Email AND IsDeleted = FALSE";

            command.Parameters.Add(new MySqlParameter("@Email", MySqlDbType.VarChar, 255) { Value = email });

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            
            if (await reader.ReadAsync(cancellationToken))
            {
                return MapCustomerFromReader(reader);
            }

            return null;
        }
        finally
        {
            if (shouldDispose)
            {
                await connection.DisposeAsync();
            }
        }
    }

    public async Task<IEnumerable<Customer>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<Customer>();

        var connection = GetConnection();
        var shouldDispose = ShouldDisposeConnection();

        try
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.Transaction = _transaction;
            command.CommandText = @"
                SELECT CustomerId, FirstName, LastName, Email, Phone, 
                       CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
                FROM Customer 
                WHERE (FirstName LIKE @SearchTerm OR LastName LIKE @SearchTerm) 
                  AND IsDeleted = FALSE
                ORDER BY FirstName, LastName
                LIMIT 100";

            command.Parameters.Add(new MySqlParameter("@SearchTerm", MySqlDbType.VarChar, 255) 
            { 
                Value = $"%{searchTerm}%" 
            });

            var customers = new List<Customer>();
            
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                customers.Add(MapCustomerFromReader(reader));
            }

            return customers;
        }
        finally
        {
            if (shouldDispose)
            {
                await connection.DisposeAsync();
            }
        }
    }

    private static Customer MapCustomerFromReader(MySqlDataReader reader)
    {
        return new Customer
        {
            CustomerId = reader.GetInt64("CustomerId"),
            FirstName = reader.GetString("FirstName"),
            LastName = reader.GetString("LastName"),
            Email = reader.GetString("Email"),
            Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone"),
            CreatedAt = reader.GetDateTime("CreatedAt"),
            CreatedBy = reader.GetString("CreatedBy"),
            UpdatedAt = reader.GetDateTime("UpdatedAt"),
            UpdatedBy = reader.GetString("UpdatedBy"),
            IsDeleted = reader.GetBoolean("IsDeleted")
        };
    }
}