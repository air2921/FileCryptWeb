interface UserDataProps {
    user: {
        username: string;
        id: string;
        role: string;
        email: string;
        is_blocked: boolean
    };
    isOwner: boolean
    showButton: boolean
}