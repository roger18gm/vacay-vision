export const checkIsAuth = (req, res, next) => {
    // Add this check at the beginning of each dashboard route
    if (!req.session.isLoggedIn) {
        req.flash('error', 'Please log in to access the dashboard');
        return res.render('accounts/login', {
            title: 'Login'
        });
    }

    next();
}

export const alreadyAuth = (req, res) => {
    if (req.session.isLoggedIn) {
        // return res.redirect('/accounts/dashboard');
        // redirect back to their previous page?
        // orrr return a bool and let the route handle redirection
    }
}

// export default checkIsAuth;