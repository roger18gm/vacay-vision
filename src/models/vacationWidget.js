import db from './db.js';

export async function createWidget(vacationId, title, type, content, external_url) {

    try {
        const result = await db.query(
            `INSERT INTO vacation_widgets (vacation_id, title, type, content, external_url)
             VALUES ($1, $2, $3, $4, $5) RETURNING *`,
            [vacationId, title, type, content, external_url || null]
        );
        return result.rows[0];

    } catch (err) {
        console.error('Error creating widget:', err.message);
    }
}

export async function getWidgetsByVacationId(vacationId) {
    try {
        const result = await db.query(
            `SELECT * FROM vacation_widgets WHERE vacation_id = $1`,
            [vacationId]
        );
        return result.rows || null;
    } catch (err) {
        console.error('Error fetching widgets:', err.message);
    }
}

export async function getWidgetById(widgetId) {
    try {
        const result = await db.query(
            `SELECT * FROM vacation_widgets WHERE vacation_widget_id = $1`,
            [widgetId]
        );
        return result.rows[0];
    } catch (err) {
        console.error('Error fetching widget:', err.message);
    }
}

export async function updateWidget(widgetId, title, type, content, external_url) {
    try {
        const result = await db.query(
            `UPDATE vacation_widgets
             SET title = $1, type = $2, content = $3, external_url = $4
             WHERE vacation_widget_id = $5
             RETURNING *`,
            [title, type, content, external_url || null, widgetId]
        );
        return result.rows[0];
    } catch (err) {
        console.error('Error updating widget:', err.message);
    }
}

export async function deleteWidget(widgetId) {
    try {
        const widget = await db.query(
            `DELETE FROM vacation_widgets
             WHERE vacation_widget_id = $1
             RETURNING vacation_id`,
            [widgetId]
        );
        return widget.rows[0]?.vacation_id || 1;
    } catch (err) {
        console.error('Error deleting widget:', err.message);
    }
}
