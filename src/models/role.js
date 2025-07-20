import db from './db.js';

export const getAllRoles = async () => {
    try {
        const query = `
            SELECT r.role_id, r.role_name
            FROM roles r;
        `;

        const result = await db.query(query);
        return result.rows;
    } catch (err) {
        console.error('Error fetching roles:', err);
    }
}

export const getRoleById = async (roleId) => {
    try {
        const query = `
            SELECT r.role_id, r.role_name
            FROM roles r
            WHERE r.role_id = $1;
        `;

        const result = await db.query(query, [roleId]);
        return result.rows[0];
    } catch (err) {
        console.error('Error fetching role by id:', err);
    }
}

export const getRoleByName = async (roleName) => {
    try {
        const query = `
            SELECT r.role_id, r.role_name
            FROM roles r
            WHERE r.role_name = $1;
        `;

        const result = await db.query(query, [roleName]);
        return result.rows[0];
    } catch (err) {
        console.error('Error fetching role by name:', err);
    }
}
