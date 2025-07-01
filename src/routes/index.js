import express from 'express';

const router = express.Router();

router.get("/", (req, res) => {
    const title = "VacayVision";
    res.render("index", { title });
});

router.get("/users/:userId", (req, res) => {

});

router.get("/user/profile", (req, res) => {
    const user = req.session.user;
    res.render("profile", { title: 'Profile', user })
});

export default router;