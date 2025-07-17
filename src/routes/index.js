import express from 'express';

const router = express.Router();

router.get("/", (req, res) => {
    const title = "VacayVision";
    res.render("index", { title });
});

export default router;