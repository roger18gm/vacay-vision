import express from 'express';

const router = express.Router();

router.get("/", (req, res) => {
    const title = "VacayVision";
    res.render("index", { title });
});

router.get("/users/:userId", (req, res) => {

});

router.get("/profile", (req, res) => {
    res.render("profile")
});

export default router;