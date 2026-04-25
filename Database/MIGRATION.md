# URL Shortener Database Migration Guide

## Database Schema

The URL Shortener application uses PostgreSQL with the following table:

### Table: `urlshortener_shorturls`
Stores the mapping between short URL identifiers and their original long URLs.

| Column | Type | Constraints |
|--------|------|-------------|
| `id` | INT | PRIMARY KEY, SERIAL |
| `url` | VARCHAR(2048) | NOT NULL |

## Migration Steps

### Using Neon DB (Quick Setup)

1. **Get your connection string from Neon dashboard**
2. **Run the schema script using psql**:
   ```bash
   psql "your_neon_connection_string" -f Database/create-tables.sql
   ```

3. **Update `appsettings.json`**:
   ```json
   {
     "ConnectionStrings": {
       "pgdb": "your_neon_connection_string"
     }
   }
   ```

4. **Test connection and run your app**:
   ```bash
   dotnet run
   ```

### Option 1: Using psql (Recommended)

1. **Create the database** (if not already created):
   ```bash
   psql -U postgres -c "CREATE DATABASE url_shortener;"
   ```

2. **Run the schema script**:
   ```bash
   psql -U postgres -d url_shortener -f Database/create-tables.sql
   ```

3. **Verify the schema** (optional):
   ```bash
   psql -U postgres -d url_shortener -c "\dt"
   ```

### Option 2: Using Connection String

If you have a connection string, you can connect directly:

```bash
psql "Host=your_host;Port=5432;Database=url_shortener;Username=your_user;Password=your_password" -f Database/create-tables.sql
```

### Option 3: Manually (GUI Tool)

1. Connect to PostgreSQL using pgAdmin or another GUI tool
2. Create a new database called `url_shortener`
3. Open the query editor
4. Copy and paste the contents of `create-tables.sql`
5. Execute the script

## Connection String Configuration

Update your `appsettings.json` with the correct connection string:

```json
{
  "ConnectionStrings": {
    "pgdb": "Host=localhost;Port=5432;Database=url_shortener;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### Connection String Examples

**Local PostgreSQL:**
```
Host=localhost;Port=5432;Database=url_shortener;Username=postgres;Password=YOUR_PASSWORD
```

**Remote PostgreSQL:**
```
Host=your_server.com;Port=5432;Database=url_shortener;Username=your_user;Password=YOUR_PASSWORD
```

**Docker PostgreSQL:**
```
Host=postgres;Port=5432;Database=url_shortener;Username=postgres;Password=YOUR_PASSWORD
```

**Azure Database for PostgreSQL:**
```
Host=your_server.postgres.database.azure.com;Port=5432;Database=url_shortener;Username=your_user@your_server;Password=YOUR_PASSWORD;SSL Mode=Require
```

**Neon DB (Serverless PostgreSQL):**
```
Host=ep-xxx-region.neon.tech;Port=5432;Database=url_shortener;Username=your_user;Password=YOUR_PASSWORD;SSL Mode=Require
```
Or use the connection string directly from your Neon dashboard (it includes all settings):
```
postgresql://your_user:YOUR_PASSWORD@ep-xxx-region.neon.tech/url_shortener?sslmode=require
```

## Importing Existing Data

If you have a backup of your data, you can restore it:

### From SQL Dump File:
```bash
psql -U postgres -d url_shortener -f your_backup.sql
```

### From Custom Data:
1. Prepare your data in this format:
   ```sql
   INSERT INTO urlshortener_shorturls (id, url) VALUES
   (1, 'https://example.com/very/long/url'),
   (2, 'https://another-domain.com/page'),
   ...
   ```

2. Execute the INSERT statements in the psql console

## Backup Your Database

Once you have your data migrated, create a backup:

```bash
# Backup just the schema
pg_dump -U postgres -d url_shortener --schema-only > schema_backup.sql

# Full backup with data
pg_dump -U postgres -d url_shortener > full_backup.sql

# Docker backup
docker exec your_postgres_container pg_dump -U postgres url_shortener > full_backup.sql
```

## Troubleshooting

### Connection Refused
- Ensure PostgreSQL is running
- Check the host, port, and credentials in your connection string
- Verify firewall rules if using a remote server

### Database Already Exists
- Drop the existing database first: `DROP DATABASE url_shortener;`
- Or use a different database name

### Permission Denied
- Ensure your user has sufficient permissions
- Use a superuser account if needed: `psql -U postgres`

### Table Already Exists
- The script uses `IF NOT EXISTS` to prevent errors
- If you need to start fresh, drop the table: `DROP TABLE urlshortener_shorturls;`

## Verifying the Migration

After running the migration script, verify everything is set up correctly:

```bash
# Connect to the database
psql -U postgres -d url_shortener

# List all tables
\dt

# Check the urlshortener_shorturls table structure
\d urlshortener_shorturls

# Count rows (should be 0 initially)
SELECT COUNT(*) FROM urlshortener_shorturls;

# Exit psql
\q
```

## Next Steps

1. Update your `appsettings.json` with the new connection string
2. Run your application: `dotnet run`
3. The Entity Framework DbContext should connect successfully to the migrated database
4. Test the URL shortener functionality with your data

## Additional Resources

- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)
