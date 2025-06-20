const flashMessages = (req, res, next) => {
    if (!req.session) {
        throw new Error("Flash middleware requires session support. Use `express-session`.");
    }

    // Initialize flash storage
    req.session.flash = req.session.flash || [];

    // Check for flash messages in cookies (from destroyed sessions)
    // Only if cookie-parser is available
    if (req.cookies) {
        const cookieFlash = req.cookies.tempFlash;
        if (cookieFlash) {
            try {
                const tempMessages = JSON.parse(decodeURIComponent(cookieFlash));
                req.session.flash.push(...tempMessages);
                // Clear the temporary cookie
                res.clearCookie('tempFlash');
            } catch (e) {
                // Invalid cookie data, just clear it
                res.clearCookie('tempFlash');
            }
        }
    }

    // Store original destroy method
    const originalDestroy = req.session.destroy.bind(req.session);

    // Override destroy to handle pending flash messages
    req.session.destroy = (callback) => {
        // If there are flash messages when destroying, save them to a cookie
        // (only if cookie functionality is available)
        if (req.session.flash && req.session.flash.length > 0 && res.cookie) {
            try {
                const flashData = encodeURIComponent(JSON.stringify(req.session.flash));
                res.cookie('tempFlash', flashData, {
                    maxAge: 60000, // 1 minute
                    httpOnly: true,
                    secure: process.env.NODE_ENV === 'production'
                });
            } catch (e) {
                // Fallback: if cookies fail, just continue with destroy
                console.warn('Flash middleware: Could not save flash messages to cookie');
            }
        }

        // Call original destroy method
        originalDestroy(callback);
    };

    // Define `req.flash` to add messages
    req.flash = (type, message) => {
        if (req.session && req.session.flash) {
            req.session.flash.push({ type, message });
        }
    };

    // Move flash messages to `res.locals.flash` for immediate use
    res.locals.flash = [...req.session.flash];

    // Clear flash messages after they have been retrieved
    req.session.flash = [];

    next();
};

export default flashMessages;