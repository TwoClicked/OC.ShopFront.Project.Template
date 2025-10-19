window.uiLogin = async (dto) => {
    const res = await fetch('/ui/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(dto),
        credentials: 'include' 
    });
    if (!res.ok) throw new Error(res.status);
    return await res.json(); // matches LoginResponseDto
};

window.uiAdminLogin = async (dto) => {
    const res = await fetch('/ui/admin/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(dto),
        credentials: 'include'
    });
    if (!res.ok) throw new Error(res.status);
    return await res.json(); // matches AdminLoginResponseDto
};

window.uiLogout = async () => {
    await fetch('/ui/logout', { method: 'POST', credentials: 'include' });
};
