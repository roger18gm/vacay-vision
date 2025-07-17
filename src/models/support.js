import db from './db.js';

// Submit feedback
export const createSupportFeedback = async (type, message, creatorId) => {
    try {
        await db.query(`
      INSERT INTO support_feedback (type, message, submitted_at, submitted_by) VALUES (
       $1, $2, CURRENT_TIMESTAMP, $3 )
    `, [type, message, creatorId]);

    } catch (err) {
        console.error('Feedback creation error:', err);
    }
}