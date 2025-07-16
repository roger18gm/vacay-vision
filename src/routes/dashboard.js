import express from 'express';
import { getAllCommunityRequestsByStatus } from '../models/communityRequest.js';
import { requireAdmin } from '../middleware/auth.js';
import db from '../models/db.js'; // prob later change this to controller imports
import { createHeadline } from '../models/headline.js';

const router = express.Router();

// Right now displays only pending but should I show approved and rejected? 
router.get('/', requireAdmin, async (req, res) => {
    try {
        const pending = await getAllCommunityRequestsByStatus('pending');
        const rejected = await getAllCommunityRequestsByStatus('rejected');
        res.render('dashboard', { title: 'Admin Dashboard', pending, rejected });
    } catch (err) {
        console.error('Error fetching dashboard:', err);
        req.flash('error', 'Error loading dashboard');
        res.redirect('/');
    }
});

// horizon headline message submission
router.post('/horizon/upload', requireAdmin, async (req, res) => {
    const creatorId = req.session.user.user_id;
    const headline = req.body.headline || null;

    try {
        const message = await createHeadline(headline, creatorId);
        req.flash('success', 'Headline Posted');
        res.redirect('/dashboard');
    } catch (err) {
        console.error('Headline creation error:', err);
        req.flash('error', 'Failed to post headline');
        res.redirect('/dashboard');
    }
});

// Approve
router.post('/:requestId/approve', requireAdmin, async (req, res) => {
    const { requestId } = req.params;
    const reviewerId = req.session.user.user_id;

    try {     // move this into a function ? 
        await db.query(`
      UPDATE community_requests
      SET status = 'approved', reviewed_at = CURRENT_TIMESTAMP, reviewed_by = $1
      WHERE request_id = $2;
    `, [reviewerId, requestId]);

        req.flash('success', 'Request approved');
        res.redirect('/dashboard');
    } catch (err) {
        console.error('Approval error:', err);
        req.flash('error', 'Failed to approve');
        res.redirect('/dashboard');
    }
});

// Reject
router.post('/:requestId/reject', requireAdmin, async (req, res) => {
    const { requestId } = req.params;
    const reviewerId = req.session.user.user_id;
    const reason = req.body.reason || null;

    try {      // move this into a function ? 
        await db.query(`
      UPDATE community_requests
      SET status = 'rejected', reviewed_at = CURRENT_TIMESTAMP, reviewed_by = $1, rejection_reason = $2
      WHERE request_id = $3;
    `, [reviewerId, reason, requestId]);

        req.flash('info', 'Request rejected');
        res.redirect('/dashboard');
    } catch (err) {
        console.error('Rejection error:', err);
        req.flash('error', 'Failed to reject');
        res.redirect('/dashboard');
    }
});


export default router;