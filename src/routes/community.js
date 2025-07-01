import express from 'express';

const router = express.Router();

router.get("/", (req, res) => {
    // Check if user is logged in
    if (!req.session.isLoggedIn) {
        req.flash('error', 'Please log in to access the community page');
        return res.redirect('/auth/login');
    }
    const title = "Community";
    const user = req.session.user;
    res.render("community", { title, user });
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


router.get("/profile", (req, res) => {
    if (!req.session.isLoggedIn) {
        req.flash('error', 'Please log in to view your profile');
        return res.redirect('/auth/login');
    }

    const title = "My Profile";
    const user = req.session.user;
    res.render("profile", { title, user });
});


export default router;