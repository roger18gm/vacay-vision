import db from './db.js';

// Submit headline
export const createHeadline = async (headline, creatorId) => {
    try {
        await db.query(`
      INSERT INTO horizon_headlines (message, created_at, created_by) VALUES (
       $1, CURRENT_TIMESTAMP, $2 )
    `, [headline, creatorId]);

        req.flash('success', 'Headline Posted');
    } catch (err) {
        console.error('Headline creation error:', err);
    }
}

// get all headlines
export const getAllHeadlines = async () => {
    try {
        const query = `
            SELECT hh.headline_id, hh.message, hh.created_at
            FROM horizon_headlines hh
            ORDER BY hh.created_at DESC;
        `;

        const result = await db.query(query);
        return result.rows;
    } catch (err) {
        console.error('Error fetching horizon headlines:', err);
    }
}

