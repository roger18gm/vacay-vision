// src/routes/vacations.js
import express from 'express';
import {
    getAllVacationByUserId, createVacation,
    getVacationById,
    updateVacationById,
    deleteVacationById
} from '../models/vacation.js';

const router = express.Router();

// Middleware to ensure login
function requireLogin(req, res, next) {
    if (!req.session.isLoggedIn) {
        req.flash('error', 'You must be logged in to view that page');
        return res.redirect('/auth/login');
    }
    next();
}

// Show form to create new vacation
router.get('/', requireLogin, async (req, res) => {
    try {
        const vacations = await getAllVacationByUserId(req.session.user.user_id);
        const id = req.session.user.user_id;
        res.render('vacations/vacations', { title: 'My Vacays', vacays: vacations, id });
    } catch (err) {
        console.error(err);
        req.flash('error', 'Failed to fetch user generated vacations');
        res.redirect('/community');
    }
});

// Show form to create new vacation
router.get('/new', requireLogin, (req, res) => {
    res.render('vacations/new', { title: 'Create Vacation' });
});

// Handle vacation creation
router.post('/', requireLogin, async (req, res) => {
    try {
        const userId = req.session.user.user_id;
        const newVacation = await createVacation(req.body, userId);
        req.flash('success', 'Vacation created successfully!');
        res.redirect('/vacations');
    } catch (err) {
        req.flash('error', 'Failed to create vacation.');
        res.redirect('/vacations/new');
    }
});

// Edit form
router.get('/:id/edit', requireLogin, async (req, res) => {
    const vacation = await getVacationById(req.params.id);
    if (!vacation) {
        req.flash('error', 'Vacation not found');
        return res.redirect('/vacations');
    }
    res.render('vacations/edit', { title: 'Edit Vacation', vacation });
});

// Update handler
router.post('/:id/update', requireLogin, async (req, res) => {
    try {
        const updated = await updateVacationById(req.params.id, req.body);
        req.flash('success', 'Vacation updated!');
        res.redirect('/vacations');
    } catch (err) {
        req.flash('error', 'Failed to update.');
        res.redirect(`/vacations/${req.params.id}/edit`);
    }
});

// Delete handler
router.post('/:id/delete', requireLogin, async (req, res) => {
    try {
        const success = await deleteVacationById(req.params.id);
        if (success) {
            req.flash('success', 'Vacation deleted.');
        } else {
            req.flash('error', 'Could not delete vacation.');
        }
        res.redirect('/vacations');
    } catch (err) {
        req.flash('error', 'Error deleting vacation.');
        res.redirect('/vacations');
    }
});

export default router;
