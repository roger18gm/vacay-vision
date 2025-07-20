import db from './db.js';

const saltRounds = 12;

/**
 * Creates a new vacation
 * @param {Object} userData - Object containing vacation data
 * @returns {Object} The created vacation
 */
async function createVacation(vacayData, userId) {
    try {
        const { title, destination, description, image_url } = vacayData;

        const query = `
            INSERT INTO vacations (title, destination, description, image_url, status, user_id)
            VALUES ($1, $2, $3, $4, $5, $6)
            RETURNING vacation_id, title, destination, description, image_url, status, user_id;
        `;

        const values = [title, destination, description, image_url, 'pending', userId];
        const result = await db.query(query, values);

        return result.rows[0];
    } catch (error) {
        console.error('Error creating vacation:', error.message);
        return error;
    }
}

/**
 * Finds a vacation by id
 * @param {number} id - id
 * @returns {Object|null} Vacay object or null if not found
 */
async function getVacationById(id) {
    try {
        const query = `
            SELECT v.vacation_id, v.title, v.destination, v.description, v.image_url, v.user_id, v.created_at
            FROM vacations v
            WHERE v.vacation_id = $1;
        `;

        const result = await db.query(query, [id]);
        return result.rows[0] || null;
    } catch (error) {
        console.error('Error fetching vacation by Id:', error.message);
        throw error;
    }
}

async function getAllVacationByUserId(userId) {
    try {
        const query = `
            SELECT v.vacation_id, v.title, v.destination, v.description, v.image_url, v.created_at
            FROM vacations v
            WHERE v.user_id = $1;
        `;

        const result = await db.query(query, [userId]);
        return result.rows;
    } catch (error) {
        console.error('Error fetching vacation by user Id:', error.message);
        throw error;
    }
}


/**
 * Updates a vacation by ID
 * @param {number} id - Vacation ID
 * @param {Object} updateData - Fields to update
 * @returns {Object|null} The updated vacation or null if not found
 */
async function updateVacationById(id, updateData) {
    try {
        const { title, destination, description, image_url, status } = updateData;

        const query = `
            UPDATE vacations
            SET title = $1,
                destination = $2,
                description = $3,
                image_url = $4,
                status = $5
            WHERE vacation_id = $6
            RETURNING vacation_id, title, destination, description, image_url, status, created_at;
        `;

        const values = [title, destination, description, image_url, status, id];
        const result = await db.query(query, values);

        return result.rows[0] || null;
    } catch (error) {
        console.error('Error updating vacation:', error.message);
        throw error;
    }
}

/**
 * Deletes a vacation by ID
 * @param {number} id - Vacation ID
 * @returns {boolean} True if deleted, false otherwise
 */
async function deleteVacationById(id) {
    try {
        const query = `
            DELETE FROM vacations
            WHERE vacation_id = $1;
        `;

        const result = await db.query(query, [id]);
        return result.rowCount > 0;
    } catch (error) {
        console.error('Error deleting vacation:', error.message);
        throw error;
    }
}


export { createVacation, getVacationById, getAllVacationByUserId, deleteVacationById, updateVacationById };