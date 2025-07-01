import express from 'express';
import { createUser, authenticateUser, emailExists } from '../models/user.js';
const router = express.Router();


router.get('/login', (req, res) => {
    // Check if user is already logged in
    if (req.session.isLoggedIn) {
        return res.redirect('/community');
    }

    res.render('login', {
        title: 'Login'
    });
});

router.post('/login', async (req, res) => {
    try {
        // const { email, password } = req.body;
        const { username: email, password } = req.body;

        // Basic validation
        if (!email || !password) {
            req.flash('error', 'Email and password are required');
            return res.render('login', {
                title: 'Login'
            });
        }

        // Authenticate user
        const user = await authenticateUser(email, password);

        if (!user) {
            req.flash('error', 'Invalid email or password');
            return res.render('login', {
                title: 'Login'
            });
        }

        // Store user information in session
        req.session.isLoggedIn = true;
        req.session.user = user;
        req.session.loginTime = new Date();

        // Flash success message and redirect
        req.flash('success', `Welcome back! You have successfully logged in.`);
        res.redirect('/community');

    } catch (error) {
        console.error('Login error:', error);
        req.flash('error', 'An error occurred during login. Please try again.');
        res.render('login', {
            title: 'Login'
        });
    }
});

router.post('/logout', (req, res) => {
    const userEmail = req.session.user?.email;

    req.session.destroy((err) => {
        if (err) {
            console.error('Error destroying session:', err);
            req.flash('error', 'Logout failed. Please try again.');
            return res.redirect('/community');
        }

        // Clear the session cookie
        res.clearCookie('sessionId');

        req.flash('success', `Goodbye! You have been successfully logged out.`);
        res.redirect('/');
    });
});

router.get('/signup', (req, res) => {
    if (req.session.isLoggedIn) {
        return res.redirect('/community');
    }

    res.render('signup', {
        title: 'Create Account'
    });
});

router.post('/signup', async (req, res) => {
    try {
        const { email, username, password, confirmPassword } = req.body;

        const errors = [];

        if (!email || !email.includes('@')) {
            errors.push('Valid email address is required');
        }

        if (!username || username.length < 8) {
            errors.push('Username must be at least 8 characters long');
        }

        if (!password || password.length < 8) {
            errors.push('Password must be at least 8 characters long');
        }

        if (password !== confirmPassword) {
            errors.push('Passwords do not match');
        }

        if (email && await emailExists(email)) {
            errors.push('An account with this email already exists');
        }

        if (errors.length > 0) {
            errors.forEach(error => req.flash('error', error));
            return res.render('signup', {
                title: 'Create Account'
            });
        }

        const newUser = await createUser({ email, username, password });

        req.flash('success', 'Account created successfully! Please log in with your new credentials.');
        res.redirect('/auth/login');

    } catch (error) {
        console.error('Registration error:', error);
        req.flash('error', 'An error occurred while creating your account. Please try again.');
        res.render('signup', {
            title: 'Create Account'
        });
    }
});

export default router;