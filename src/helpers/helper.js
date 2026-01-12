
export const formatCreatedAt = (date) => {
    if (!date || typeof date !== 'string') return '';

    const dateObj = new Date(date);
    return `${dateObj.toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' })}`;
}