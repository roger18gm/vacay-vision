import express from 'express';
import { getAllVacationByUserId } from '../models/vacation.js';
import { getUserById } from '../models/user.js';

const router = express.Router();

router.get("/", (req, res) => {
    const title = "VacayVision";
    res.render("index", { title });
});

router.get("/users/:userId", async (req, res) => {
    const userId = req.params.userId;
    const user = await getUserById(userId);
    const vacations = await getAllVacationByUserId(userId);

    // if (!user) {
    //     req.flash('error', 'User not found');
    //     return res.redirect('/community');
    // }

    res.render('vacations/vacations', { title: `${user.username}'s Profile`, vacays: vacations, id: userId });

});

router.get("/user/profile", (req, res) => {
    const user = req.session.user;
    res.render("profile", { title: 'Profile', user })
});

export default router;