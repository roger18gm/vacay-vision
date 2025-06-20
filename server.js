import { fileURLToPath } from 'url';
import path from 'path';
import express from 'express';

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


// Routes


// Error Handling
// Catch-all middleware for unmatched routes (404)
app.use((req, res, next) => {
    const err = new Error('Page Not Found');
    err.status = 404;
    next(err); // Forward to the global error handler
});

// Global error handler middleware
app.use((err, req, res, next) => {
    console.error(err.stack);

    // Set default status and determine error type
    const status = err.status || 500;
    const title = status === 404 ? 'Page Not Found' : 'Internal Server Error';
    const error = err.message;
    const stack = err.stack;

    // Render the appropriate template based on status code
    res.status(status).render(`errors/${status === 404 ? '404' : '500'}`, { title, error, stack });
});

// When in development mode, start a WebSocket server for live reloading
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