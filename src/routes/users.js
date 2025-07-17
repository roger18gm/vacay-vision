import express from 'express';
import { getAllVacationByUserId } from '../models/vacation.js';
import { getUserById } from '../models/user.js';
import { createSupportFeedback } from '../models/support.js';

const router = express.Router();

router.get("/users/test", async (req, res) => {
    const user = await getUserById(153); // known existing user

    console.log(user); // is it null?

    res.json(user);
});

router.get("/user/:userId", async (req, res) => {
    // const userId = req.params.userId;
    // const userId = parseInt(req.params.userId);
    console.log("Original URL:", req.originalUrl);
    console.log("req.params:", req.params);
    console.log("typeof req.params.userId:", typeof req.params.userId);
    const userId = parseInt(req.params.userId, 10);
    console.log("Parsed userId:", userId, "Type:", typeof userId);

    const sessionUser = req.session.user;

    try {

        const user = await getUserById(userId);
        console.log("Fetched user from DB:", user);

        if (!user) {
            req.flash("error", "User not found");
            return res.redirect("/");
        }
        const vacations = await getAllVacationByUserId(userId);
        const isOwnProfile = sessionUser && sessionUser.user_id === userId;
        const feedbackTypes = ["Bug", "Suggestion", "Feedback", "Other"];

        res.render("users/profile", {
            title: `${user.username}'s Profile`, user, vacays: vacations, isOwnProfile, types: isOwnProfile ? feedbackTypes : []
        });
    } catch (err) {
        console.error("Error fetching user profile:", err);
        req.flash("error", "Error loading user profile");
        res.redirect("/");
    }
});

router.post("/user/:userId", async (req, res) => {
    const sessionUserId = req.session.user.user_id;
    const { type, message } = req.body;
    try {
        const supportFeedback = await createSupportFeedback(type, message, sessionUserId);
        req.flash('success', 'Feedback Sent');
        res.redirect(`/users/${sessionUserId}`);
    } catch (err) {
        req.flash("error", "Error submitting support feedback");
    }
});

export default router;
