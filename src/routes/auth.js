import express from 'express';
import { createUser, authenticateUser, emailExists } from '../models/accounts/index.js';
const router = express.Router();


router.get('/login', (req, res) => {
    // Check if user is already logged in
    if (req.session.isLoggedIn) {
        return res.redirect('/accounts/dashboard');
    }

    res.render('accounts/login', {
        title: 'Login'
    });
});

router.post('/login', async (req, res) => {
    try {
        const { username: email, password } = req.body;

        // Basic validation
        if (!email || !password) {
            req.flash('error', 'Email and password are required');
            return res.render('accounts/login', {
                title: 'Login'
            });
        }

        // Authenticate user
        const user = await authenticateUser(email, password);

        if (!user) {
            req.flash('error', 'Invalid email or password');
            return res.render('accounts/login', {
                title: 'Login'
            });
        }

        // Store user information in session
        req.session.isLoggedIn = true;
        req.session.user = user;
        req.session.loginTime = new Date();

        // Flash success message and redirect
        req.flash('success', `Welcome back! You have successfully logged in.`);
        res.redirect('/accounts/dashboard');

    } catch (error) {
        console.error('Login error:', error);
        req.flash('error', 'An error occurred during login. Please try again.');
        res.render('accounts/login', {
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
            return res.redirect('/accounts/dashboard');
        }

        // Clear the session cookie
        res.clearCookie('sessionId');

        // Flash success message and redirect to home
        req.flash('success', `Goodbye! You have been successfully logged out.`);
        res.redirect('/');
    });
});

router.post('/register', async (req, res) => {
    try {
        const { email, password, confirmPassword } = req.body;

        // Basic validation
        const errors = [];

        if (!email || !email.includes('@')) {
            errors.push('Valid email address is required');
        }

        if (!password || password.length < 8) {
            errors.push('Password must be at least 8 characters long');
        }

        if (password !== confirmPassword) {
            errors.push('Passwords do not match');
        }

        // Check if email already exists
        if (email && await emailExists(email)) {
            errors.push('An account with this email already exists');
        }

        // If validation errors exist, redisplay the form
        if (errors.length > 0) {
            errors.forEach(error => req.flash('error', error));
            return res.render('accounts/register', {
                title: 'Create Account'
            });
        }

        // Create the user account
        const newUser = await createUser({ email, password });

        // Flash success message and redirect to login
        req.flash('success', 'Account created successfully! Please log in with your new credentials.');
        res.redirect('/accounts/login');

    } catch (error) {
        console.error('Registration error:', error);
        req.flash('error', 'An error occurred while creating your account. Please try again.');
        res.render('accounts/register', {
            title: 'Create Account'
        });
    }
});