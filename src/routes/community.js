import express from 'express';
import { getAllVacationByUserId } from '../models/vacation.js';
import { requireLogin } from '../middleware/auth.js';

const router = express.Router();

router.get("/", (req, res) => {
    // Check if user is logged in
    if (!req.session.isLoggedIn) {
        req.flash('error', 'Please log in to access the community page');
        return res.redirect('/auth/login');
    }
    const title = "Community";
    const user = req.session.user;
    res.render("community/community", { title, user });
});

router.get("/users/:userId", async (req, res) => {
    const userId = req.params.userId;
    const user = await getUserById(userId); // you'll implement this
    const vacations = await getVacationsByUserId(userId);

    if (!user) {
        req.flash('error', 'User not found');
        return res.redirect('/community');
    }

    res.render("user-profile", {
        title: `${user.username}'s Profile`,
        user,
        vacations
    });
});

//community/submit
router.get("/submit", requireLogin, async (req, res) => {
    const vacations = await getAllVacationByUserId(req.session.user.user_id);
    const title = "Create Community Vacay Request";
    const step = req.query.step || 'step-1'; // default to step-1 if not provided
    res.render("community/vacay-submit", { title, vacations, step });
});


export default router;