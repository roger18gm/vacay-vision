import db from './db.js';

/**
 * SQL to create the roles table if it does not exist.
 * This table defines user permission levels:
 * 0 = Regular user, 1 = Moderator, 2 = Admin
 */
const createRolesTable = `
    CREATE TABLE IF NOT EXISTS roles (
        role_id INTEGER PRIMARY KEY,
        role_name VARCHAR(50) NOT NULL UNIQUE
    );
`;

/**
 * SQL to insert default roles if they do not exist.
 */
const insertDefaultRoles = `
    INSERT INTO roles (role_id, role_name) VALUES 
        (0, 'user'),
        (1, 'moderator'), 
        (2, 'admin')
    ON CONFLICT (role_id) DO NOTHING;
`;

/**
 * SQL to create the users table if it does not exist.
 * This table stores user account information including email,
 * hashed password, and role assignment.
 */
const createUsersTable = `
CREATE TABLE IF NOT EXISTS users (
    user_id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    bio TEXT,
    password VARCHAR(255) NOT NULL,
    role_id INTEGER DEFAULT 0 REFERENCES roles(role_id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
`;

/**
 * SQL to insert default user.
 */
const insertDefaultUser = `
    INSERT INTO users (username, email, bio, password, role_id) VALUES 
        ('demo', 'user@example.com', 'demo bio', 'thepassword', 2)
    ON CONFLICT (email) DO NOTHING;
`;

/**
 * SQL to create the vacations table if it doesn't exist.
 */
const createVacationsTable = `
    CREATE TABLE IF NOT EXISTS vacations (
        vacation_id SERIAL PRIMARY KEY,
        title VARCHAR(100) NOT NULL,
        status VARCHAR(100) NOT NULL,
        destination VARCHAR(100) NOT NULL,
        description TEXT,
        image_url VARCHAR(500) NOT NULL,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        user_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE
    );
`;

/**
 * SQL to insert a default vacation.
 */
const insertTestVacation = `
    INSERT INTO vacations (vacation_id, title, status, destination, description, image_url, user_id) VALUES 
        (0, 'Island Escape', 'pending', 'Mexico', 'Dreaming of turquoise waters and white sand beaches.', 'https://example.com/beach.jpg', 1)
    ON CONFLICT (vacation_id) DO NOTHING;
`;


/**
 * SQL to create the vacations widgets table if it doesn't exist.
 */
const createVacationWidgetsTable = `
    CREATE TABLE IF NOT EXISTS vacation_widgets (
        vacation_widget_id SERIAL PRIMARY KEY,
        vacation_id INTEGER NOT NULL REFERENCES vacations(vacation_id) ON DELETE CASCADE,
        type VARCHAR(255) NOT NULL,
        content TEXT NOT NULL
    );
`;

/**
 * SQL to insert a widget linked to the 'Island Escape' vacation.
 */
const insertTestVacationWidget = `
    INSERT INTO vacation_widgets (vacation_widget_id, vacation_id, type, content) VALUES (
        0,
        (SELECT vacation_id FROM vacations WHERE title = 'Island Escape' LIMIT 1),
        'location',
        'Bora Bora, French Polynesia'
    )
    ON CONFLICT (vacation_widget_id) DO NOTHING;
`;

const createCommunityRequestTable = `
    CREATE TABLE IF NOT EXISTS community_requests (
        request_id SERIAL PRIMARY KEY,
        vacation_id INT NOT NULL REFERENCES vacations(vacation_id) ON DELETE CASCADE,
        user_id INT NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
        status VARCHAR(50) NOT NULL DEFAULT 'pending', -- can be 'pending', 'approved', 'rejected'
        submitted_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        reviewed_at TIMESTAMP,
        reviewed_by INT REFERENCES users(user_id),
        rejection_reason TEXT
    );
`;


/**
 * Sets up the database by creating tables and inserting initial data.
 * This function should be called when the server starts.
 */
const setupDatabase = async () => {
    const verbose = process.env.DISABLE_SQL_LOGGING !== 'true';

    try {
        if (verbose) console.log('Setting up database...');

        // Create the roles table
        await db.query(createRolesTable);
        if (verbose) console.log('Roles table ready');

        // Insert default roles
        await db.query(insertDefaultRoles);
        if (verbose) console.log('Default roles inserted');

        // Create the users table
        await db.query(createUsersTable);
        if (verbose) console.log('Users table ready');

        // Insert default user
        await db.query(insertDefaultUser);
        if (verbose) console.log('Default user inserted');

        // Create the categories table
        await db.query(createVacationsTable);
        if (verbose) console.log('Vacations table ready');

        // Create the products table (add this after categories table creation)
        await db.query(createVacationWidgetsTable);
        if (verbose) console.log('Vacation Widgets table ready');

        await db.query(createCommunityRequestTable);
        if (verbose) console.log('Community Requests Widgets table ready');

        // Insert test vacation
        await db.query(insertTestVacation);
        if (verbose) console.log('Test Vacation inserted');

        // Insert test vacation widget
        await db.query(insertTestVacationWidget);
        if (verbose) console.log('Test Vacation Widget inserted');

        if (verbose) console.log('Database setup complete');
        return true;
    } catch (error) {
        console.error('Error setting up database:', error.message);
        throw error;
    }
};

/**
 * Tests the database connection by executing a simple query.
 */
const testConnection = async () => {
    try {
        const result = await db.query('SELECT NOW() as current_time');
        console.log('Database connection successful:', result.rows[0].current_time);
        return true;
    } catch (error) {
        console.error('Database connection failed:', error.message);
        throw error;
    }
};

export { setupDatabase, testConnection };