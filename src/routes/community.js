import express from 'express';
import { getAllVacationByUserId } from '../models/vacation.js';
import { requireLogin } from '../middleware/auth.js';
import { getAllCommunityRequestsByStatus, submitCommunityRequest } from '../models/communityRequest.js';

const router = express.Router();

router.get("/", async (req, res) => {
    // Check if user is logged in
    if (!req.session.isLoggedIn) {
        req.flash('error', 'Please log in to access the community page');
        return res.redirect('/auth/login');
    }
    const title = "Community";
    const user = req.session.user;
    const rows = await getAllCommunityRequestsByStatus('approved');
    res.render("community/community", { title, user, approved: rows });
});

//community/submit
router.get("/submit", requireLogin, async (req, res) => {
    const title = "Create Community Vacay Request";
    const vacations = await getAllVacationByUserId(req.session.user.user_id);
    const step = req.query.step || 'select'; // default to step-1 if not provided
    const selectedVacationId = req.query.vacationId;
    res.render("community/vacay-submit", { title, vacations, step, selectedVacationId });
});

// Final submission POST
router.post('/submit', requireLogin, async (req, res) => {
    const { step } = req.query;
    const { vacationId } = req.body;
    const userId = req.session.user.user_id;

    if (step === 'submit') {
        try {
            await submitCommunityRequest(vacationId, userId);

            req.flash('success', 'Vacation submitted for community review!');
            return res.redirect('/community/submit?step=submitted');
        } catch (error) {
            console.error('Error submitting vacation to community:', error);
            req.flash('error', 'There was an error. Please try again.');
            return res.redirect('/community/submit?step=confirm&vacationId=' + vacationId);
        }
    }

    res.redirect('/community/submit');
});

export default router;