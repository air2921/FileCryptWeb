import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

const useResize = () => {
    const [width, setWidth] = useState(window.innerWidth);
    const navigate = useNavigate();

    useEffect(() => {
        const handleResize = (event: Event) => {
            setWidth((event.target as Window).innerWidth);
        };
        window.addEventListener('resize', handleResize);
        return () => {
            window.removeEventListener('resize', handleResize);
        };
    }, [navigate]);

    return width >= 768;
}

export default useResize;