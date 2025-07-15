export const authGlobalData = async (req, res, next) => {
    res.locals.isLoggedIn = req.session.isLoggedIn || false;
    res.locals.currentUser = req.session.user || null;
    next();
};

// export default checkIsAuth;

// Middleware to ensure login
export const requireLogin = (req, res, next) => {
    if (!req.session.isLoggedIn) {
        req.flash('error', 'You must be logged in to view that page');
        return res.redirect('/auth/login');
    }
    next();
}

// Only allow admins
export const requireAdmin = (req, res, next) => {
    if (req.session.user && req.session.user.role_id === 2) {
        return next();
    }
    req.flash('error', 'You must be an admin to access this page.');
    res.redirect('/');
}