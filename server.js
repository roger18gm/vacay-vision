import { fileURLToPath } from 'url';
import path from 'path';
import express from 'express';
import session from 'express-session';
import pgSession from 'connect-pg-simple';
import { addGlobalData } from './src/middleware/globals.js';
import indexRoutes from './src/routes/index.js';
import authRoutes from './src/routes/auth.js';
import dashboardRoutes from './src/routes/dashboard.js';
import vacationsRoutes from './src/routes/vacations.js';
import communityRoutes from './src/routes/community.js';
import { setupDatabase, testConnection } from './src/models/setup.js';
import db from './src/models/db.js';
import flashMessages from './src/middleware/flash.js';
import { authGlobalData } from './src/middleware/auth.js';

// Setup
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const NODE_ENV = process.env.NODE_ENV || 'production';
const PORT = process.env.PORT || 3000;
const app = express();

app.set('view engine', 'ejs');
app.use(express.static(path.join(__dirname, 'public')));
app.set('views', path.join(__dirname, 'src/views'));

// Middleware
app.use(express.json());
app.use(express.urlencoded({ extended: true }));
app.use(addGlobalData);
const PostgresStore = pgSession(session);
app.use(session({
    store: new PostgresStore({
        pool: db,// postgres database connection
        tableName: 'sessions',
        createTableIfMissing: true
    }),
    secret: process.env.SESSION_SECRET || "default-secret-change-in-production",
    resave: false,
    saveUninitialized: false,
    name: "sessionId",
    cookie: {
        secure: false, // Set to true in production with HTTPS
        httpOnly: true, // Prevents client-side access to the cookie
        maxAge: 30 * 24 * 60 * 60 * 1000 // 30 days in milliseconds
    }
}));
app.use(flashMessages);
app.use(authGlobalData); // used auth user data. tried including with addGlobalData but errors..

// Routes
app.use("/", indexRoutes);
app.use("/auth", authRoutes);
app.use("/community", communityRoutes);
app.use('/vacations', vacationsRoutes);
app.use("/dashboard", dashboardRoutes);

// Error Handling
app.use((req, res, next) => {
    const err = new Error('Page Not Found');
    err.status = 404;
    next(err);
});

// Global error handler middleware
app.use((err, req, res, next) => {
    console.error(err.stack);
    const status = err.status || 500;
    const title = status === 404 ? 'Page Not Found' : 'Internal Server Error';
    const error = err.message;
    const stack = err.stack;
    res.status(status).render(`errors/${status === 404 ? '404' : '500'}`, { title, error, stack });
});

if (NODE_ENV.includes('dev')) {
    const ws = await import('ws');

    try {
        const wsPort = parseInt(PORT) + 1;
        const wsServer = new ws.WebSocketServer({ port: wsPort });

        wsServer.on('listening', () => {
            console.log(`WebSocket server is running on port ${wsPort}`);
        });
        wsServer.on('error', (error) => {
            console.error('WebSocket server error:', error);
        });
    } catch (error) {
        console.error('Failed to start WebSocket server:', error);
    }
}

app.listen(PORT, async () => {
    try {
        await testConnection();
        await setupDatabase();
    } catch (error) {
        console.error('Database setup failed:', error);
        process.exit(1);
    }
    console.log(`Server is running on http://127.0.0.1:${PORT}`);
});