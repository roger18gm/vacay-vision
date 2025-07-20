import express from 'express';
import { getAllVacationByUserId } from '../models/vacation.js';
import { requireLogin } from '../middleware/auth.js';
import { getAllCommunityRequestsByStatus, getAllCommunityRequestsByUserId, submitCommunityRequest } from '../models/communityRequest.js';
import { getAllHeadlines } from '../models/headline.js';

const router = express.Router();

router.get("/", async (req, res) => {
    if (!req.session.isLoggedIn) {
        req.flash('error', 'Please log in to access the community page');
        return res.redirect('/auth/login');
    }
    const title = "Community";
    const user = req.session.user;
    const rows = await getAllCommunityRequestsByStatus('approved');
    const headlines = await getAllHeadlines();
    res.render("community/community", { title, headlines, user, approved: rows });
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

router.get("/my-submissions", requireLogin, async (req, res) => {
    const title = "My Community Submission History";
    const vacations = await getAllCommunityRequestsByUserId(req.session.user.user_id);
    res.render("community/statusHistory", { title, vacations });
});

export default router;