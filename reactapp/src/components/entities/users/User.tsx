import React, { useEffect, useState } from "react";
import imageCompression from 'browser-image-compression';
import { UserProps } from "../../../utils/api/Users";
import { resizeImage } from "../../../utils/helpers/Formatter";
import { useNavigate } from "react-router-dom";

function User({ user, isOwner, avatar_size, file }: { user: UserProps, isOwner: boolean, avatar_size: number, file: File }) {
    const [avatar, setAvatar] = useState<string | null>();

    const navigate = useNavigate();

    async function formatImage() {
        try {

            const options = {
                maxSizeMB: 3,
                useWebWorker: true,
            };

            const compressedFile = await imageCompression(file, options);
            const resizedImage = await resizeImage(compressedFile, 400, 400);
            const resizedImageUrl = URL.createObjectURL(resizedImage);
            setAvatar(resizedImageUrl);

        } catch (error: any) {
            console.log(error);
        }
    }

    useEffect(() => {
        formatImage();
    }, []);

    return (
        <div className="entity-user-container">
            <div>{user.is_blocked && <div className="entity-user-blocked">The user is currently blocked</div>}</div>
            <div className="entity-user-avatar">
                <a className="entity-avatar-tooltip" aria-label="Change your avatar" href="/settings">
                    {avatar && <img src={avatar} style={{ width: `${avatar_size}px`, height: `${avatar_size}px`, borderRadius: '50%', objectFit: 'cover' }}></img>}
                </a>
            </div>
            <div className="entity-user-props">
                <div className="entity-user-main">
                    <div>{user.username}#{user.id}</div>
                    <div>{user.role}</div>
                    <div>{isOwner && user.email && (<>{user.email}</>)}</div>
                </div>
            </div>
            <div>{isOwner && <button onClick={() => navigate('/settings')}>Edit Profile</button>}</div>
        </div>
    );
}

export default User;