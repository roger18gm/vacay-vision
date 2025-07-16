import db from './db.js';

// Save a community request (create this table if needed)
export const submitCommunityRequest = async (vacationId, userId) => {
    try {
        await db.query(
            `INSERT INTO community_requests (vacation_id, user_id, status, submitted_at)
         VALUES ($1, $2, $3, CURRENT_TIMESTAMP)`,
            [vacationId, userId, 'pending']
        );
    } catch (error) {
        console.error('Error submitting community request', error.message);
        throw error;
    }
}

export const getAllCommunityRequestsByStatus = async (status) => {
    try {
        const query = `
            SELECT cr.request_id, cr.status, cr.submitted_at,
                    v.title, v.destination, v.description, v.image_url,
                    u.username, u.user_id
            FROM community_requests cr
            JOIN vacations v ON cr.vacation_id = v.vacation_id
            JOIN users u ON cr.user_id = u.user_id
            WHERE cr.status = $1
            ORDER BY cr.submitted_at DESC;
        `;

        const result = await db.query(query, [status]);
        return result.rows;
    } catch (err) {
        console.error('Error fetching community requests:', err);
    }
}