import db from "./db.js";

export async function addComment(vacationId, userId, content) {
    try {

        const result = await db.query(
            `INSERT INTO vacation_comments (vacation_id, user_id, content)
         VALUES ($1, $2, $3)
         RETURNING *`,
            [vacationId, userId, content]
        );
        return result.rows[0];
    } catch (err) {
        console.error(err);
    }
}

export async function getCommentsByVacationId(vacationId) {
    try {
        const result = await db.query(
            `SELECT vc.*, u.username
            FROM vacation_comments vc
            JOIN users u ON vc.user_id = u.user_id
            WHERE vc.vacation_id = $1
            ORDER BY vc.created_at DESC`,
            [vacationId]
        );
        return result.rows;
    } catch (err) {
        console.error(err);
    }

}