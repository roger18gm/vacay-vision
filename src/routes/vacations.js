// src/routes/vacations.js
import express from 'express';
import {
    getAllVacationByUserId, createVacation,
    getVacationById,
    updateVacationById,
    deleteVacationById
} from '../models/vacation.js';
import { requireLogin } from '../middleware/auth.js';
import { createWidget, deleteWidget, getWidgetById, getWidgetsByVacationId, updateWidget } from '../models/vacationWidget.js';
import { getUserById } from '../models/user.js';

const router = express.Router();

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

router.get('/:vacationId', requireLogin, async (req, res) => {
    const vacationId = parseInt(req.params.vacationId);

    try {
        const vacation = await getVacationById(vacationId);
        const widgets = await getWidgetsByVacationId(vacationId);
        const vacationUser = await getUserById(vacation.user_id);
        const userId = req.session.user.user_id;
        res.render('vacations/vacationDetail', { title: vacation.title, vacation, widgets: widgets === null ? [] : widgets, userId, vacationUser });
    } catch (err) {
        console.error(err);
        req.flash('error', 'There was an error getting this vacation.');
        res.redirect('/community');
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

router.post('/:vacationId/widgets', requireLogin, async (req, res) => {
    const { vacationId } = req.params;
    const { title, type, content, external_url } = req.body;

    try {
        const result = await createWidget(vacationId, title, type, content, external_url);
        res.redirect(`/vacations/${vacationId}`);
        req.flash('success', 'Widget created successfully');
    } catch (err) {
        console.error('Error in POST:', err.message);
        req.flash('error', ' Error creating widget');
        res.status(500).send('Server error creating widget');
    }
});

router.put('/widgets/:widgetId', requireLogin, async (req, res) => {
    const { widgetId } = req.params;
    const { title, type, content, external_url } = req.body;

    try {
        const result = await updateWidget(widgetId, title, type, content, external_url);
        console.log(result);
        req.flash('success', 'Widget updated successfully');
        res.redirect(`/vacations/${result.vacation_id}`);
    } catch (err) {
        console.error('Error in PUT /widgets/:widgetId:', err.message);
        req.flash('error', ' Error editing widget');
        res.status(500).send('Server error editing widget');
    }
});

router.delete('/widgets/:widgetId', requireLogin, async (req, res) => {
    const widgetId = req.params.widgetId;

    try {
        const widget = await getWidgetById(widgetId);
        const vacationId = widget.vacation_id;
        const deletedWidget = await deleteWidget(widgetId);
        req.flash('success', 'Widget Deleted');
        res.redirect(`/vacations/${vacationId}`);
    } catch (err) {
        console.error('Error deleting widget:', err.message);
        req.flash('error', 'Error Deleting Widget');
        res.status(500).send('Server error deleting widget');
    }
});
export default router;
