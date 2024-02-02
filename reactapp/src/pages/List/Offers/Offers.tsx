import React, { FormEvent, useEffect, useState } from 'react';
import OfferList from '../../../components/List/OfferList/OfferList';
import Input from '../../../components/Helpers/Input';
import AxiosRequest from '../../../api/AxiosRequest';
import Message from '../../../components/Message/Message';
import Button from '../../../components/Helpers/Button';
import Font from '../../../components/Font/Font';

const Offers = () => {
    const [errorMessage, setErrorMessage] = useState('');
    const [offersList, setOffers] = useState(null);
    const [userId, setUserId] = useState<number>();
    const [skip, setSkip] = useState(0);
    const step = 10;
    const [message, setMessage] = useState('');
    const [font, setFont] = useState('');

    const [lastOfferModified, setLastOfferModified] = useState(Date.now());

    const fetchData = async () => {
        const response = await AxiosRequest({ endpoint: `api/core/offers/all?skip=${skip}&count=${step}`, method: 'GET', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setOffers(response.data);
        }
        else {
            setErrorMessage(response.data);
        }
    }

    const handleLoadMore = () => {
        setSkip(prevSkip => prevSkip + step);
    };

    const handleBack = () => {
        setSkip(prevSkip => Math.max(0, prevSkip - step));
    };

    const createOffer = async (e: FormEvent) => {
        e.preventDefault();

        const response = await AxiosRequest({ endpoint: `api/core/offers/new/${userId}`, method: 'POST', withCookie: true, requestBody: null })

        if (response.isSuccess) {
            setMessage(response.data.message);
            setFont('done');
            setLastOfferModified(Date.now());
        }
        else {
            setMessage(response.data)
            setFont('error');
        }

        setTimeout(() => {
            setMessage('');
            setFont('');
        }, 5000)
    }

    const deleteOffer = async (offerId: number) => {
        const response = await AxiosRequest({ endpoint: `api/core/offers/${offerId}`, method: 'DELETE', withCookie: true, requestBody: null })

        if (response.isSuccess) {
            setMessage('');
            setFont('');
            setLastOfferModified(Date.now());
        }
        else {
            setMessage(response.data);
            setFont('error');
        }

        setTimeout(() => {
            setMessage('');
            setFont('');
        }, 5000)
    }

    const acceptOffer = async (offerId: number) => {
        const response = await AxiosRequest({ endpoint: `api/core/offers/accept/${offerId}`, method: 'PUT', withCookie: true, requestBody: null })

        if (response.isSuccess) {
            setMessage('');
            setFont('');
            setLastOfferModified(Date.now());
        }
        else {
            setMessage(response.data);
            setFont('error');
        }

        setTimeout(() => {
            setMessage('');
            setFont('');
        }, 5000)
    }

    useEffect(() => {
        fetchData();
    }, [lastOfferModified, skip]);

    if (!offersList) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { offers, user_id } = offersList as { offers: any[], user_id: number }

    return (
        <div className="container">
            <div className="create">
                <form onSubmit={createOffer}>
                    <Input text='UID of the offer receiver' type="number" id="offer" require={true} value={userId} onChange={(e) => setUserId(parseInt(e.target.value, 10))} />
                    <Button>Submit</Button>
                </form>
            </div>
            <div className="offers">
                <OfferList offers={offers} user_id={user_id} isOwner={true} deleteOffer={deleteOffer} acceptOffer={acceptOffer} />
                {skip > 0 && <Button onClick={handleBack}><Font font={'arrow_back'} /></Button>}
                {offers.length > step - 1 && <Button onClick={handleLoadMore}><Font font={'arrow_forward'} /></Button>}
            </div>
            {message && font && < Message message={message} font={font} />}
        </div>
    );
}

export default Offers;