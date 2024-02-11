import React, { FormEvent, useEffect, useState } from 'react';
import OfferList from '../../../components/List/OfferList/OfferList';
import AxiosRequest from '../../../api/AxiosRequest';
import Message from '../../../components/Message/Message';

const Offers = () => {
    const [skip, setSkip] = useState(0);
    const step = 10;
    const [orderBy, setOrderBy] = useState('true');
    const [isSent, setSent] = useState('');
    const [isAccepted, setAccepted] = useState('');
    const [type, setType] = useState('');

    const [errorMessage, setErrorMessage] = useState('');
    const [offersList, setOffers] = useState(null);
    const [message, setMessage] = useState('');
    const [font, setFont] = useState('');

    const [lastOfferModified, setLastOfferModified] = useState(Date.now());

    const fetchData = async () => {

        const response = await AxiosRequest({
            endpoint: `api/core/offers/all?skip=${skip}&count=${step}&byDesc=${orderBy}&sended=${isSent}&isAccepted=${isAccepted}&type=${type}`,
            method: 'GET',
            withCookie: true,
            requestBody: null
        });

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
    }, [lastOfferModified, skip, orderBy, isSent, isAccepted, type]);

    if (!offersList) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { offers, user_id } = offersList as { offers: any[], user_id: number }

    const CreateOffer = () => {
        const [userId, setUserId] = useState<number>();

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

        return (
            <div className="create">
                <form onSubmit={createOffer}>
                    <label htmlFor="offer">
                        ID user you want send offer
                        <input
                            type="text"
                            id="code"
                            required={true}
                            value={userId}
                            onChange={(e) => {
                                const value = e.target.value;
                                if (value === '') {
                                    setUserId(undefined);
                                } else {
                                    const parsedValue = parseInt(value, 10);
                                    if (!isNaN(parsedValue)) {
                                        setUserId(parsedValue);
                                    }
                                }
                            }}
                            inputMode="numeric"
                            placeholder="User ID"
                        />
                    </label>
                    <button>Submit</button>
                </form>
            </div>
        );
    }

    const SortOffers = () => {
        return (
            <div className="sort">
                <details className="accepted">
                    <summary>
                        <span>Is accepted</span>
                    </summary>
                    <select
                        className="accepted"
                        id="accepted"
                        required={true}
                        value={isAccepted}
                        onChange={(e) => setAccepted(e.target.value)}>
                        <option value="">All</option>
                        <option value="true">Only accepted</option>
                        <option value="false">Only non-accepted</option>
                    </select>
                </details>
                <details className="sent">
                    <summary>
                        <span>Is sent</span>
                    </summary>
                    <select
                        className="sent"
                        id="sent"
                        required={true}
                        value={isSent}
                        onChange={(e) => setSent(e.target.value)}>
                        <option value="">All</option>
                        <option value="true">Sent</option>
                        <option value="false">Received</option>
                    </select>
                </details>
                <details className="type">
                    <summary>
                        <span>Type</span>
                    </summary>
                    <select
                        className="offer-type"
                        id="type"
                        required={true}
                        value={type}
                        onChange={(e) => setType(e.target.value)}>
                        <option value="">All</option>
                        <option value="Key">Key</option>
                    </select>
                </details>
                <details className="order-by">
                    <summary>
                        <span>Order by</span>
                    </summary>
                    <select
                        className="order-by"
                        id="order"
                        required={true}
                        value={orderBy}
                        onChange={(e) => setOrderBy(e.target.value)}>
                        <option value="true">Order by descending</option>
                        <option value="false">Order by ascending</option>
                    </select>
                </details>
            </div>
        );
    }

    return (
        <div className="container">
            <CreateOffer />
            <div className="offers">
                <SortOffers />
                <OfferList offers={offers} user_id={user_id} isOwner={true} deleteOffer={deleteOffer} acceptOffer={acceptOffer} />
                <div className="scroll">
                    {skip > 0 && <button onClick={handleBack}>Previous</button>}
                    {offers.length > step - 1 && <button onClick={handleLoadMore}>Next</button>}
                </div>
            </div>
            <div className="message">
                {message && font && < Message message={message} font={font} />}
            </div>
        </div>
    );
}

export default Offers;